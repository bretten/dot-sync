using System.Collections.Concurrent;
using System.Collections.Immutable;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Contracts;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Execution;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Exceptions;
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
    /// Job execution context
    /// </summary>
    private readonly JobExecutionContext _jobContext;

    /// <summary>
    /// Reports the progress of the job
    /// </summary>
    private readonly IJobProgressReporter _jobProgressReporter;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fileRepository">Stores the expected state of the files</param>
    /// <param name="fileMetadataReader">Metadata reader used to get the date of the file</param>
    /// <param name="fileChecksumGenerator">Generates checksums for files</param>
    /// <param name="logger">Logger</param>
    /// <param name="jobContext">Job execution context</param>
    /// <param name="jobProgressReporter">Reports the progress of the job</param>
    public LocalFileSystemScanner(IFileRepository fileRepository, IFileMetadataReader fileMetadataReader,
        IFileChecksumGenerator fileChecksumGenerator, ILogger<IFileSystemScanner> logger,
        JobExecutionContext jobContext, IJobProgressReporter jobProgressReporter)
    {
        _fileRepository = fileRepository;
        _fileMetadataReader = fileMetadataReader;
        _fileChecksumGenerator = fileChecksumGenerator;
        _logger = logger;
        _jobContext = jobContext;
        _jobProgressReporter = jobProgressReporter;
    }

    /// <inheritdoc />
    public async Task<FileSystemScannerResult> Scan(StorageLocation storageLocation, string pathPrefix)
    {
        if (storageLocation.Type != StorageLocationType.Local)
        {
            throw new IncompatibleStorageLocationException("Not a local storage location");
        }

        var pathToScan = !string.IsNullOrWhiteSpace(pathPrefix)
            ? Path.Combine(storageLocation.Path.Value, pathPrefix)
            : storageLocation.Path.Value;
        var tasks = ScanDirectory(new DirectoryInfo(pathToScan), storageLocation.Path).ToList();
        var newFiles = new ConcurrentBag<DotFile>();
        var completedTasks = 0;
        var taskExecutions = tasks.Select(async task =>
        {
            var result = await task();

            Interlocked.Increment(ref completedTasks);
            _jobProgressReporter.ReportPercent(this, _jobContext.Id, completedTasks, tasks.Count);

            if (result != null) newFiles.Add(result);
        });
        await Task.WhenAll(taskExecutions);

        foreach (var newFile in newFiles)
        {
            await _fileRepository.Add(newFile);
        }

        return new FileSystemScannerResult(newFiles.ToImmutableList());
    }

    /// <summary>
    /// Scans the specified directory for new files
    /// </summary>
    /// <param name="directoryInfo">The directory to scan</param>
    /// <param name="rootDirectoryPath">The original root directory that is being scanned</param>
    /// <returns>New files found in the directory</returns>
    private IEnumerable<Func<Task<DotFile?>>> ScanDirectory(DirectoryInfo directoryInfo, StoragePath rootDirectoryPath)
    {
        var entries = directoryInfo.EnumerateFileSystemInfos();
        var tasks = new List<Func<Task<DotFile?>>>();
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
                    tasks.Add(() => ScanFile(info, rootDirectoryPath));
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
    private async Task<DotFile?> ScanFile(FileInfo fileInfo, StoragePath rootDirectoryPath)
    {
        // Determine its relative path compared to the root directory
        var relativePath = FileSystemPath.Create(Path.GetRelativePath(rootDirectoryPath.Value, fileInfo.FullName));

        // See if the file's path exists
        var existingFileByPath = await _fileRepository.GetFileByPath(relativePath);
        if (existingFileByPath != null)
        {
            // The file exists, no further work needed
            return null;
        }

        using (_logger.BeginScope(new List<KeyValuePair<string, object>>()
               {
                   new(nameof(JobExecutionContext), _jobContext.Id)
               }))
        {
            _logger.LogInformation($"Scanning {fileInfo.FullName}");
        }

        // File creation time (or best estimation)
        var fileCreation = _fileMetadataReader.ReadFileCreationDate(fileInfo.FullName);

        // Generate a checksum for the new file
        var checksum = FileSha256Checksum.Create(_fileChecksumGenerator.GenerateChecksum(fileInfo));

        // The file could not be found via checksum or file path. It is a new file
        return new DotFile(Guid.NewGuid(), relativePath, checksum, fileInfo.Length, fileCreation);
    }
}