using System.Collections.Concurrent;
using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.Common.Application.Jobs.Extensions;
using com.brettnamba.DotSync.FileSystem.Application.Configuration;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;

/// <summary>
/// Verifies the integrity of files on a local filesystem
/// </summary>
public sealed class LocalFileSystemFileIntegrityVerifier : BaseFileIntegrityVerifier
{
    /// <summary>
    /// Reads metadata for new files
    /// </summary>
    private readonly IFileMetadataReader _metadataReader;

    /// <summary>
    /// Execution context for the current job
    /// </summary>
    private readonly JobExecutionContext _jobContext;

    /// <summary>
    /// Reports job progress
    /// </summary>
    private readonly IJobProgressReporter _jobProgressReporter;

    /// <summary>
    /// Configured directories to skip
    /// </summary>
    private readonly DirectoryConfiguration _directoryConfiguration;

    public LocalFileSystemFileIntegrityVerifier(IFileRepository fileRepository,
        IFileChecksumGenerator checksumGenerator, ILogger<IFileIntegrityVerifier> logger,
        IFileMetadataReader metadataReader, JobExecutionContext jobContext, IJobProgressReporter jobProgressReporter,
        IOptions<DirectoryConfiguration> directoryConfiguration) : base(fileRepository, checksumGenerator, logger)
    {
        _metadataReader = metadataReader;
        _jobContext = jobContext;
        _jobProgressReporter = jobProgressReporter;
        _directoryConfiguration = directoryConfiguration.Value;
    }

    /// <inheritdoc/>
    protected override async Task<IEnumerable<FileIntegrityVerificationResult>> VerifyDirectory(
        StorageLocation storageLocation, string pathPrefix, IEnumerable<string> pathsToSkip)
    {
        // Existing files to verify
        var dbFiles = await FileRepository.GetFilesByPathPrefix(pathPrefix);
        var trackedFiles = new TrackedFiles(dbFiles);
        var skips = pathsToSkip.ToList();

        // Skip the sort dir
        skips.Add(_directoryConfiguration.SortDir);

        // Path to verify
        var rootPath = storageLocation.Path.Value;
        var dirPath = Path.Combine(rootPath, pathPrefix);
        var results = new ConcurrentBag<FileIntegrityVerificationResult>();
        var completedTasks = 0;

        // Verify tasks
        var tasks = VerifyDirectory(rootPath, new DirectoryInfo(dirPath), skips, trackedFiles).ToList();
        var taskExecutions = tasks.Select(async task =>
        {
            var result = await task();

            Interlocked.Increment(ref completedTasks);
            _jobProgressReporter.ReportPercent(this, _jobContext.Id, completedTasks, tasks.Count);

            results.Add(result);
        });
        await Task.WhenAll(taskExecutions);

        // Any leftover tracked files that could not be verified are considered missing
        foreach (var trackedFile in trackedFiles.AllFiles)
        {
            if (skips.Any(x => trackedFile.Path.StartsWith(x))) continue;
            results.Add(FileIntegrityVerificationResult.Missing(FileSystemPath.Create(trackedFile.Path),
                FileSha256Checksum.Create(trackedFile.Checksum), 0));
        }

        return results;
    }

    /// <summary>
    /// Verifies the integrity of all files within the specified directory
    /// </summary>
    /// <param name="rootPath">The root path</param>
    /// <param name="directoryInfo">The directory to verify</param>
    /// <param name="pathsToSkip">Paths to skip</param>
    /// <param name="trackedFiles">Currently tracked files</param>
    /// <returns>Verification results for each file within the directory</returns>
    private IEnumerable<Func<Task<FileIntegrityVerificationResult>>> VerifyDirectory(string rootPath,
        DirectoryInfo directoryInfo, List<string> pathsToSkip, TrackedFiles trackedFiles)
    {
        // Determine this directory's relative path compared to the root directory to see if it should be skipped
        var relativePath = Path.GetRelativePath(rootPath, directoryInfo.FullName);
        if (pathsToSkip.Contains(relativePath))
        {
            Logger.LogInformation($"Skipping path {relativePath}");
            return Array.Empty<Func<Task<FileIntegrityVerificationResult>>>();
        }

        var entries = directoryInfo.EnumerateFileSystemInfos();
        var tasks = new List<Func<Task<FileIntegrityVerificationResult>>>();
        foreach (var entry in entries)
        {
            if (entry.Attributes.HasFlag(FileAttributes.Hidden) && !entry.Name.Contains(".medresframes"))
            {
                Logger.LogWarning($"Skipping hidden file {entry.FullName}");
                continue;
            }

            switch (entry)
            {
                case FileInfo info:
                    tasks.Add(() => VerifyFile(rootPath, info, trackedFiles));
                    break;
                case DirectoryInfo info:
                    tasks.AddRange(VerifyDirectory(rootPath, info, pathsToSkip, trackedFiles));
                    break;
            }
        }

        return tasks;
    }

    /// <summary>
    /// Verifies the integrity of a single file
    /// </summary>
    /// <param name="rootPath">The root path</param>
    /// <param name="fileInfo">The file</param>
    /// <param name="trackedFiles">Currently tracked files</param>
    /// <returns>Verification result of the file</returns>
    private Task<FileIntegrityVerificationResult> VerifyFile(string rootPath, FileInfo fileInfo,
        TrackedFiles trackedFiles)
    {
        Logger.LogWithScope($"Verifying {fileInfo.FullName}", _jobContext);

        // Generate the checksum of the file on the filesystem
        var checksum = FileSha256Checksum.Create(ChecksumGenerator.GenerateChecksum(fileInfo));
        // Determine its relative path compared to the root directory
        var relativePath = FileSystemPath.Create(Path.GetRelativePath(rootPath, fileInfo.FullName));

        var pathExists = trackedFiles.Paths.Contains(relativePath.Value);
        var checksumExists = trackedFiles.ChecksumsToPaths.ContainsKey(checksum.Value);
        var checksumVerified = checksumExists && trackedFiles.ChecksumsToPaths[checksum.Value] == relativePath.Value;

        // The file is new
        if (!pathExists && !checksumExists)
        {
            // File creation time (or best estimation)
            var fileCreation = _metadataReader.ReadFileCreationDate(fileInfo.FullName);
            return Task.FromResult(
                FileIntegrityVerificationResult.New(relativePath, checksum, fileInfo.Length, fileCreation));
        }

        trackedFiles.ExcludeTrackedFile(new TrackedFile(relativePath.Value, checksum.Value));

        // Short circuit the most important status
        if (pathExists && checksumVerified)
        {
            // The file is verified
            return Task.FromResult(FileIntegrityVerificationResult.Verified(relativePath, checksum, fileInfo.Length));
        }


        if (!pathExists && checksumExists)
        {
            // The file moved
            return Task.FromResult(FileIntegrityVerificationResult.Moved(relativePath, checksum, fileInfo.Length));
        }
        else
        {
            // The checksum has changed
            return Task.FromResult(FileIntegrityVerificationResult.Unverified(relativePath, checksum, fileInfo.Length));
        }
    }

    private record TrackedFile(string Path, string Checksum);

    /// <summary>
    /// Collection of all the currently tracked files
    /// </summary>
    private record TrackedFiles
    {
        /// <summary>
        /// Tracked file paths
        /// </summary>
        public HashSet<string> Paths { get; private set; }

        /// <summary>
        /// Mapping of tracked file checksums to paths
        /// </summary>
        public Dictionary<string, string> ChecksumsToPaths { get; private set; }

        /// <summary>
        /// Currently tracked files
        /// </summary>
        public HashSet<TrackedFile> AllFiles { get; private set; }

        public TrackedFiles(IEnumerable<DotFile> files)
        {
            var list = files.ToList();
            AllFiles = new HashSet<TrackedFile>(list.Select(x =>
                new TrackedFile(x.Path.Value, x.Sha256Checksum.Value)));
            Paths = new HashSet<string>(list.Select(x => x.Path.Value));
            ChecksumsToPaths = list.ToDictionary(x => x.Sha256Checksum.Value, x => x.Path.Value);
        }

        public void ExcludeTrackedFile(TrackedFile trackedFile)
        {
            AllFiles.Remove(trackedFile);
        }
    };
}