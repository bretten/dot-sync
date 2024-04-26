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
    /// Logger
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fileMetadataReader">Metadata reader used to get the date of the file</param>
    /// <param name="logger">Logger</param>
    public LocalFileSystemByDateFileSorter(IFileMetadataReader fileMetadataReader, ILogger logger)
    {
        _fileMetadataReader = fileMetadataReader;
        _logger = logger;
    }

    /// <summary>
    /// <inheritdoc cref="IFileSorter.Sort"/>
    /// </summary>
    public Task Sort(FileSystemPath sourcePath, FileSystemPath destinationPath)
    {
        var sourceDirectoryInfo = new DirectoryInfo(sourcePath.Value);

        // Get all directories that are within the source path
        var sourceDirectories = sourceDirectoryInfo.EnumerateFileSystemInfos()
            .OfType<DirectoryInfo>();

        // Each directory in the source path will be used to determine where each file will be sorted
        foreach (var sourceDirectory in sourceDirectories)
        {
            // Get the files in the source dir
            var files = GetFilesInDirectory(sourceDirectory);
            // Move the files to the destination
            MoveFiles(sourceDirectory, files, destinationPath);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets all files within a directory
    /// </summary>
    private IEnumerable<FileInfo> GetFilesInDirectory(DirectoryInfo directory)
    {
        var files = new List<FileInfo>();
        foreach (var entry in directory.EnumerateFileSystemInfos())
        {
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
    private void MoveFiles(DirectoryInfo sourceDirectory, IEnumerable<FileInfo> files, FileSystemPath destinationPath)
    {
        var containingDirectories = new List<DirectoryInfo>();
        foreach (var file in files)
        {
            // Get the date of the file
            var date = _fileMetadataReader.ReadPhotoOrVideoTakenDate(FileSystemPath.Create(file.FullName));

            // Get the directory of the file's containing folder so we can delete it when it becomes empty
            if (file.Directory != null) containingDirectories.Add(file.Directory);

            // Move the file
            MoveFile(sourceDirectory, date, destinationPath, file);
        }

        // Remove the containing directories
        foreach (var containingDirectory in containingDirectories)
        {
            _logger.LogInformation($"Deleting containing directory {containingDirectory.FullName}");
            containingDirectory.Delete();
        }
    }

    /// <summary>
    /// Moves the specified file
    /// </summary>
    /// <param name="sourceDirectory">The original top-level directory of the file</param>
    /// <param name="fileDate">The date of the file</param>
    /// <param name="destinationPath">The destination path</param>
    /// <param name="file">The file to move</param>
    /// <exception cref="FileAlreadyExistsAtMoveDestinationException">Thrown if there is already a file at the specified location</exception>
    private void MoveFile(DirectoryInfo sourceDirectory, DateTime fileDate, FileSystemPath destinationPath,
        FileInfo file)
    {
        // If the file was located at path/to/file.txt, the sourceDirectory would be "path"
        var destinationFolderName = $"{sourceDirectory.Name} - {fileDate:yyyy-MM}";
        var newPath = Path.Combine(destinationPath.Value, fileDate.Year.ToString(), destinationFolderName, file.Name);
        if (File.Exists(newPath))
        {
            throw new FileAlreadyExistsAtMoveDestinationException($"File already exists at {newPath}");
        }

        // Create the directories if they don't exist
        new FileInfo(newPath).Directory?.Create();

        _logger.LogInformation($"Moving {file.FullName} to {newPath}");
        File.Move(file.FullName, newPath);
    }

    private sealed class FileAlreadyExistsAtMoveDestinationException(string? message) : Exception(message);
}