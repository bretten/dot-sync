using System.Collections.Concurrent;
using System.Collections.Immutable;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.Services;

/// <summary>
/// Scans the local filesystem for new files
/// </summary>
public sealed class LocalFileSystemScanner : IFileSystemScanner
{
    /// <summary>
    /// Stores the expected state of the files
    /// </summary>
    private readonly IFileRepository _fileRepository;

    /// <summary>
    /// Metadata reader used to get the date of the file
    /// </summary>
    private readonly IFileMetadataReader _fileMetadataReader;

    /// <summary>
    /// Generates checksums for files
    /// </summary>
    private readonly IFileChecksumGenerator _fileChecksumGenerator;

    /// <summary>
    /// Logger
    /// </summary>
    private readonly ILogger<IFileSystemScanner> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fileRepository">Stores the expected state of the files</param>
    /// <param name="fileMetadataReader">Metadata reader used to get the date of the file</param>
    /// <param name="fileChecksumGenerator">Generates checksums for files</param>
    /// <param name="logger">Logger</param>
    public LocalFileSystemScanner(IFileRepository fileRepository, IFileMetadataReader fileMetadataReader,
        IFileChecksumGenerator fileChecksumGenerator, ILogger<IFileSystemScanner> logger)
    {
        _fileRepository = fileRepository;
        _fileMetadataReader = fileMetadataReader;
        _fileChecksumGenerator = fileChecksumGenerator;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<FileSystemScannerResult> Scan(FileSystemPath path)
    {
        var tasks = ScanDirectory(new DirectoryInfo(path.Value), path);
        var newFiles = new ConcurrentBag<Tuple<FileSystemPath, FileSha256Checksum>>();
        await Parallel.ForEachAsync(tasks, async (task, token) =>
        {
            var result = await task;
            if (result != null) newFiles.Add(result);
        });
        return new FileSystemScannerResult(newFiles.ToImmutableList());
    }

    /// <summary>
    /// Scans the specified directory for new files
    /// </summary>
    /// <param name="directoryInfo">The directory to scan</param>
    /// <param name="rootDirectoryPath">The original root directory that is being scanned</param>
    /// <returns>New files found in the directory</returns>
    private IEnumerable<Task<Tuple<FileSystemPath, FileSha256Checksum>?>> ScanDirectory(DirectoryInfo directoryInfo,
        FileSystemPath rootDirectoryPath)
    {
        var entries = directoryInfo.EnumerateFileSystemInfos();
        var tasks = new List<Task<Tuple<FileSystemPath, FileSha256Checksum>?>>();
        foreach (var entry in entries)
        {
            if (entry.Attributes.HasFlag(FileAttributes.Hidden))
            {
                _logger.LogWarning($"Skipping hidden file {entry.FullName}");
                continue;
            }

            switch (entry)
            {
                case FileInfo info:
                    tasks.Add(ScanFile(info, rootDirectoryPath));
                    break;
                case DirectoryInfo info:
                    tasks.AddRange(ScanDirectory(info, rootDirectoryPath));
                    break;
            }
        }

        return tasks;
    }

    /// <summary>
    /// Checks if a file is not yet synced with the domain
    /// </summary>
    /// <param name="fileInfo">The file</param>
    /// <param name="rootDirectoryPath">The original root directory of the file</param>
    /// <returns><see cref="DotFile"/> if it is a new file, otherwise null</returns>
    private async Task<Tuple<FileSystemPath, FileSha256Checksum>?> ScanFile(FileInfo fileInfo,
        FileSystemPath rootDirectoryPath)
    {
        // Determine its relative path compared to the root directory
        var relativePath = FileSystemPath.Create(Path.GetRelativePath(rootDirectoryPath.Value, fileInfo.FullName),
            replaceBackslashes: OperatingSystem.IsWindows());

        // See if the file's path exists
        var existingFileByPath = await _fileRepository.GetFileByPath(relativePath);
        if (existingFileByPath != null)
        {
            // The file exists, no further work needed
            return null;
        }
        _logger.LogInformation($"Scanning {fileInfo.FullName}");

        // File creation time (or best estimation)
        var fileCreation = _fileMetadataReader.ReadFileCreationDate(FileSystemPath.Create(fileInfo.FullName));

        // Generate a checksum for the new file
        var checksum = FileSha256Checksum.Create(_fileChecksumGenerator.GenerateChecksum(fileInfo));

        // The file could not be found via checksum or file path. It is a new file, so add it
        var newFile = new DotFile(Guid.NewGuid(), relativePath, checksum, fileInfo.Length, fileCreation);
        await _fileRepository.Add(newFile);
        return new Tuple<FileSystemPath, FileSha256Checksum>(relativePath, checksum);
    }
}