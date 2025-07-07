using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Domain.FileOrganization.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs;

/// <summary>
/// Hangfire implementation of the job manager
/// </summary>
public sealed class HangfireJobManager : IJobManager
{
    private readonly IFileSystemScanner _fileSystemScanner;
    private readonly IStorageLocationIntegrityVerificationService _verifier;
    private readonly IFilePusher _filePusher;
    private readonly IFileSorter _fileSorter;
    private readonly IBackgroundJobClient _jobClient;
    private readonly IJobResultProvider _jobResultProvider;
    private readonly ILogger<HangfireJobManager> _logger;

    public HangfireJobManager(IFileSystemScanner fileSystemScanner,
        IStorageLocationIntegrityVerificationService verifier, IFilePusher filePusher, IFileSorter fileSorter,
        IBackgroundJobClient jobClient, IJobResultProvider jobResultProvider, ILogger<HangfireJobManager> logger)
    {
        _fileSystemScanner = fileSystemScanner;
        _verifier = verifier;
        _filePusher = filePusher;
        _fileSorter = fileSorter;
        _jobClient = jobClient;
        _jobResultProvider = jobResultProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task RunJob(IJobParameters jobParameters)
    {
        switch (jobParameters)
        {
            case ScanParameters scan:
                if (IsJobRunning(nameof(Scan)))
                {
                    _logger.LogInformation("Job is already running");
                    break;
                }

                _jobClient.Enqueue(() => Scan(scan));
                break;
            case VerifyParameters verify:
                if (IsJobRunning(nameof(Verify)))
                {
                    _logger.LogInformation("Job is already running");
                    break;
                }

                _jobClient.Enqueue(() => Verify(verify));
                break;
            case PushParameters push:
                if (IsJobRunning(nameof(Push)))
                {
                    _logger.LogInformation("Job is already running");
                    break;
                }

                _jobClient.Enqueue(() => Push(push));
                break;
            case SortParameters sort:
                if (IsJobRunning(nameof(Sort)))
                {
                    _logger.LogInformation("Job is already running");
                    break;
                }

                _jobClient.Enqueue(() => Sort(sort));
                break;
            default:
                throw new ArgumentException("Unknown job type");
        }

        return Task.CompletedTask;
    }

    public async Task Scan(ScanParameters parameters)
    {
        var result = await _fileSystemScanner.Scan(FileSystemPath.Create(parameters.Path ?? ""));
        _jobResultProvider.OnJobCompleted(new JobResult(parameters, result));
    }

    public async Task Verify(VerifyParameters parameters)
    {
        var result = await _verifier.Execute(parameters.StorageType,
            FileSystemPath.Create(parameters.StoragePath ?? ""),
            FileSystemPath.Create(parameters.VerifyPath ?? ""),
            !string.IsNullOrWhiteSpace(parameters.PathsToSkip)
                ? parameters.PathsToSkip.Split(',', StringSplitOptions.TrimEntries).Select(FileSystemPath.Create)
                : new List<FileSystemPath>());
        _jobResultProvider.OnJobCompleted(new JobResult(parameters, result));
    }

    public async Task Push(PushParameters parameters)
    {
        var result = await _filePusher.PushFilesInDir(StorageLocationType.Local,
            FileSystemPath.Create(parameters.SourceRootPath ?? ""),
            FileSystemPath.Create(parameters.SourcePushPath ?? ""),
            parameters.DestinationType,
            FileSystemPath.Create(parameters.DestinationPath ?? ""));
        _jobResultProvider.OnJobCompleted(new JobResult(parameters, result));
    }

    public async Task Sort(SortParameters parameters)
    {
        var result = await _fileSorter.Sort(FileSystemPath.Create(parameters.SourcePath ?? ""),
            FileSystemPath.Create(parameters.DestinationPath ?? ""));
        _jobResultProvider.OnJobCompleted(new JobResult(parameters, result));
    }

    private bool IsJobRunning(string jobName)
    {
        var monitoringApi = JobStorage.Current.GetMonitoringApi();
        var processingJobs = monitoringApi.ProcessingJobs(0, int.MaxValue);

        return processingJobs.Any(job => job.Value.Job.Method.Name == jobName);
    }
}