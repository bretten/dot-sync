using System.Collections.Concurrent;
using com.brettnamba.DotSync.FileSystem.Domain.FileOrganization.Exceptions;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileOrganization.Services;

/// <summary>
/// Sorts files by their date on a local filesystem
/// </summary>
public sealed class LocalFileSystemByDateFileSorter : IFileSorter
{
    /// <summary>
    /// Metadata reader used to get the date of the file
    /// </summary>
    private readonly IFileMetadataReader _fileMetadataReader;

    /// <summary>
    /// Used to get local storage locations to prevent sorting directly on them
    /// </summary>
    private readonly IStorageLocationRepository _storageLocationRepository;

    /// <summary>
    /// Logger
    /// </summary>
    private readonly ILogger<IFileSorter> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fileMetadataReader">Metadata reader used to get the date of the file</param>
    /// <param name="storageLocationRepository">Used to get local storage locations to prevent sorting directly on them</param>
    /// <param name="logger">Logger</param>
    public LocalFileSystemByDateFileSorter(IFileMetadataReader fileMetadataReader,
        IStorageLocationRepository storageLocationRepository, ILogger<IFileSorter> logger)
    {
        _fileMetadataReader = fileMetadataReader;
        _storageLocationRepository = storageLocationRepository;
        _logger = logger;
    }

    /// <summary>
    /// <inheritdoc cref="IFileSorter.Sort"/>
    /// </summary>
    public async Task<IEnumerable<string>> Sort(StoragePath sourcePath, StoragePath destinationPath)
    {
        var localStorageLocations = (await _storageLocationRepository.GetAll())
            .Where(x => x.Type == StorageLocationType.Local).ToList();
        if (localStorageLocations.Any(x => x.Path == sourcePath))
        {
            throw new SortPathSameAsStoragePathException($"Sort path matches a local storage: {sourcePath.Value}");
        }

        var sourceDirectoryInfo = new DirectoryInfo(sourcePath.Value);

        // Each of the top-level directories in the source path determines where the file will be moved to
        var sourceDirectories = sourceDirectoryInfo.EnumerateFileSystemInfos()
            .OfType<DirectoryInfo>();

        // Determine all the files in the source path that will be moved (minimal IO work, parallelize)
        var totalFilesToMove = 0;
        var filesToMoveByDirectory = new ConcurrentDictionary<DirectoryInfo, List<FileInfo>>();
        Parallel.ForEach(sourceDirectories, sourceDirectory =>
        {
            // Get the files in the top-level source dir
            var filesToMove = GetFilesInDirectory(sourceDirectory).ToList();
            Interlocked.Add(ref totalFilesToMove, filesToMove.Count); // Update the total count

            filesToMoveByDirectory[sourceDirectory] = filesToMove;
        });

        // Move each file within the top-level source dir to the corresponding destination
        var result = new List<string>(); // Files that were successfully moved
        var filesMoved = 0;
        foreach (var (sourceDirectory, files) in filesToMoveByDirectory)
        {
            // Move the files to the destination
            result.AddRange(MoveFiles(sourceDirectory, files, destinationPath, ref filesMoved));
        }

        return result.AsEnumerable();
    }

    /// <summary>
    /// Gets all files within a directory
    /// </summary>
    private IEnumerable<FileInfo> GetFilesInDirectory(DirectoryInfo directory)
    {
        var files = new List<FileInfo>();
        foreach (var entry in directory.EnumerateFileSystemInfos())
        {
            if (entry.Attributes.HasFlag(FileAttributes.Hidden))
            {
                Console.WriteLine($"Skipping hidden file {entry.FullName}");
                continue;
            }

            switch (entry)
            {
                case FileInfo info:
                    files.Add(info);
                    break;
                case DirectoryInfo info:
                    files.AddRange(GetFilesInDirectory(info));
                    break;
            }
        }

        return files;
    }

    /// <summary>
    /// Moves the specified files to the destination path
    /// </summary>
    /// <param name="sourceDirectory">The original top-level directory of the file</param>
    /// <param name="files">The files to be moved</param>
    /// <param name="destinationPath">The destination</param>
    /// <param name="filesMoved">Counter for the total files moved so far</param>
    /// <returns>Files that were moved</returns>
    private List<string> MoveFiles(DirectoryInfo sourceDirectory, IEnumerable<FileInfo> files,
        StoragePath destinationPath, ref int filesMoved)
    {
        var result = new List<string>();
        var containingDirectories = new List<DirectoryInfo>();
        foreach (var file in files)
        {
            // Get the date of the file
            var date = _fileMetadataReader.ReadFileCreationDate(file.FullName);

            // Get the directory of the file's containing folder so we can delete it when it becomes empty
            if (file.Directory != null) containingDirectories.Add(file.Directory);

            // Move the file
            result.Add(MoveFile(sourceDirectory, date, destinationPath, file));
            filesMoved++;
        }

        // Remove the containing directories
        foreach (var containingDirectory in containingDirectories)
        {
            _logger.LogInformation($"Deleting containing directory {containingDirectory.FullName}");
            try
            {
                containingDirectory.Delete();
            }
            catch (DirectoryNotFoundException)
            {
                // Directory is already removed
            }
        }

        return result;
    }

    /// <summary>
    /// Moves the specified file
    /// </summary>
    /// <param name="sourceDirectory">The original top-level directory of the file</param>
    /// <param name="fileDate">The date of the file</param>
    /// <param name="destinationPath">The destination path</param>
    /// <param name="file">The file to move</param>
    /// <returns>New path</returns>
    /// <exception cref="FileAlreadyExistsAtMoveDestinationException">Thrown if there is already a file at the specified location</exception>
    private string MoveFile(DirectoryInfo sourceDirectory, DateTime fileDate, StoragePath destinationPath,
        FileInfo file)
    {
        // If the file was located at path/to/file.txt, the sourceDirectory would be "path"
        var destinationFolderName = $"{sourceDirectory.Name} - {fileDate:yyyy-MM}";
        var newPath = Path.Combine(destinationPath.Value, fileDate.Year.ToString(), fileDate.ToString("yyyy-MM"),
            destinationFolderName, file.Name);
        if (File.Exists(newPath))
        {
            throw new FileAlreadyExistsAtMoveDestinationException($"File already exists at {newPath}");
        }

        // Create the directories if they don't exist
        new FileInfo(newPath).Directory?.Create();

        _logger.LogInformation($"Moving {file.FullName} to {newPath}");
        File.Move(file.FullName, newPath);
        return newPath;
    }

    private sealed class FileAlreadyExistsAtMoveDestinationException(string? message) : Exception(message);
}