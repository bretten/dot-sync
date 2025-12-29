using System.Collections.Concurrent;
using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;

/// <summary>
/// Verifies the integrity of files on a local filesystem
/// </summary>
public sealed class LocalFileSystemFileIntegrityVerifier(
    IFileRepository fileRepository,
    IFileChecksumGenerator checksumGenerator,
    ILogger<IFileIntegrityVerifier> logger,
    IFileMetadataReader metadataReader,
    FileSystemPath rootPath,
    JobExecutionContext jobContext,
    IJobProgressReporter jobProgressReporter)
    : BaseFileIntegrityVerifier(fileRepository, checksumGenerator, logger)
{
    /// <summary>
    /// Verifies the integrity of all files within the specified directory
    /// </summary>
    /// <param name="directoryPath">The path to the directory that will be verified</param>
    /// <param name="pathsToSkip">Paths to skip</param>
    /// <returns>Verification results for each file within the directory</returns>
    protected override async Task<IEnumerable<FileIntegrityVerificationResult>> VerifyDirectory(
        FileSystemPath directoryPath, IEnumerable<FileSystemPath> pathsToSkip)
    {
        var dbFiles = await FileRepository.GetFilesByPath(directoryPath);
        var trackedFiles = new TrackedFiles(dbFiles);
        var skips = pathsToSkip.ToList();

        var dirPath = Path.Combine(rootPath.Value, directoryPath.Value);
        var tasks = VerifyDirectory(new DirectoryInfo(dirPath), skips, trackedFiles).ToList();
        var results = new ConcurrentBag<FileIntegrityVerificationResult>();
        var completedTasks = 0;
        await Parallel.ForEachAsync(tasks, async (task, token) =>
        {
            var result = await task;
            Interlocked.Increment(ref completedTasks);
            jobProgressReporter.ReportPercent(this, jobContext.Id, completedTasks, tasks.Count);
            results.Add(result);
        });

        // Any leftover tracked files that could not be verified are considered missing
        foreach (var trackedFile in trackedFiles.AllFiles)
        {
            if (skips.Any(x => trackedFile.Path.StartsWith(x.Value))) continue;
            results.Add(FileIntegrityVerificationResult.Missing(FileSystemPath.Create(trackedFile.Path),
                FileSha256Checksum.Create(trackedFile.Checksum), 0));
        }

        return results;
    }

    /// <summary>
    /// Verifies the integrity of all files within the specified directory
    /// </summary>
    /// <param name="directoryInfo">The directory to verify</param>
    /// <param name="pathsToSkip">Paths to skip</param>
    /// <param name="trackedFiles">Currently tracked files</param>
    /// <returns>Verification results for each file within the directory</returns>
    private IEnumerable<Task<FileIntegrityVerificationResult>> VerifyDirectory(DirectoryInfo directoryInfo,
        List<FileSystemPath> pathsToSkip, TrackedFiles trackedFiles)
    {
        // Determine this directory's relative path compared to the root directory to see if it should be skipped
        var relativePath = FileSystemPath.Create(Path.GetRelativePath(rootPath.Value, directoryInfo.FullName),
            replaceBackslashes: OperatingSystem.IsWindows());
        if (pathsToSkip.Contains(relativePath))
        {
            return Array.Empty<Task<FileIntegrityVerificationResult>>();
        }

        var entries = directoryInfo.EnumerateFileSystemInfos();
        var tasks = new List<Task<FileIntegrityVerificationResult>>();
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
                    Logger.LogInformation($"Verifying {info.FullName}");
                    tasks.Add(VerifyFile(info, trackedFiles));
                    break;
                case DirectoryInfo info:
                    tasks.AddRange(VerifyDirectory(info, pathsToSkip, trackedFiles));
                    break;
            }
        }

        return tasks;
    }

    /// <summary>
    /// Verifies the integrity of a single file
    /// </summary>
    /// <param name="fileInfo">The file</param>
    /// <param name="trackedFiles">Currently tracked files</param>
    /// <returns>Verification result of the file</returns>
    private async Task<FileIntegrityVerificationResult> VerifyFile(FileInfo fileInfo, TrackedFiles trackedFiles)
    {
        // Generate the checksum of the file on the filesystem
        var checksum = FileSha256Checksum.Create(ChecksumGenerator.GenerateChecksum(fileInfo));
        // Determine its relative path compared to the root directory
        var relativePath = FileSystemPath.Create(Path.GetRelativePath(rootPath.Value, fileInfo.FullName),
            replaceBackslashes: OperatingSystem.IsWindows());

        var pathExists = trackedFiles.Paths.Contains(relativePath.Value);
        var checksumExists = trackedFiles.ChecksumsToPaths.ContainsKey(checksum.Value);
        var checksumVerified = checksumExists && trackedFiles.ChecksumsToPaths[checksum.Value] == relativePath.Value;

        // The file is new
        if (!pathExists && !checksumExists)
        {
            // File creation time (or best estimation)
            var fileCreation = metadataReader.ReadFileCreationDate(FileSystemPath.Create(fileInfo.FullName));
            return FileIntegrityVerificationResult.New(relativePath, checksum, fileInfo.Length, fileCreation);
        }

        trackedFiles.ExcludeTrackedFile(new TrackedFile(relativePath.Value, checksum.Value));

        // Short circuit the most important status
        if (pathExists && checksumVerified)
        {
            // The file is verified
            return FileIntegrityVerificationResult.Verified(relativePath, checksum, fileInfo.Length);
        }


        if (!pathExists && checksumExists)
        {
            // The file moved
            return FileIntegrityVerificationResult.Moved(relativePath, checksum, fileInfo.Length);
        }
        else
        {
            // The checksum has changed
            return FileIntegrityVerificationResult.Unverified(relativePath, checksum, fileInfo.Length);
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