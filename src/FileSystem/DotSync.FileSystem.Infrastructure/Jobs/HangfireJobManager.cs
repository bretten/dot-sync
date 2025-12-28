using System.Collections.Immutable;
using System.Text.Json;
using com.brettnamba.DotSync.FileSystem.Application.Files;
using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Domain.FileOrganization.Exceptions;
using com.brettnamba.DotSync.FileSystem.Domain.FileOrganization.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Configuration;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs;

/// <summary>
/// Hangfire implementation of the job manager
/// </summary>
public sealed class HangfireJobManager : IJobManager
{
    private readonly IFileSystemScanner _fileSystemScanner;
    private readonly IStorageLocationRepository _storageLocationRepo;
    private readonly IStorageLocationIntegrityVerificationService _verifier;
    private readonly IFilePusher _filePusher;
    private readonly IFileSorter _fileSorter;
    private readonly IThumbnailProvider _thumbnailProvider;
    private readonly IBackgroundJobClient _jobClient;
    private readonly IJobResultProvider _jobResultProvider;
    private readonly JobConfiguration _jobConfiguration;
    private readonly ILogger<HangfireJobManager> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public const string DefaultSortDir = "ToUpload";

    public HangfireJobManager(IFileSystemScanner fileSystemScanner, IStorageLocationRepository storageLocationRepo,
        IStorageLocationIntegrityVerificationService verifier, IFilePusher filePusher, IFileSorter fileSorter,
        IThumbnailProvider thumbnailProvider, IBackgroundJobClient jobClient, IJobResultProvider jobResultProvider,
        JobConfiguration jobConfiguration, ILogger<HangfireJobManager> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _fileSystemScanner = fileSystemScanner;
        _storageLocationRepo = storageLocationRepo;
        _verifier = verifier;
        _filePusher = filePusher;
        _fileSorter = fileSorter;
        _thumbnailProvider = thumbnailProvider;
        _jobClient = jobClient;
        _jobResultProvider = jobResultProvider;
        _jobConfiguration = jobConfiguration;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
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
            case PushByStorageParameters pushByStorage:
                if (IsJobRunning(nameof(Push)))
                {
                    _logger.LogInformation("Job is already running");
                    break;
                }

                _jobClient.Enqueue(() => Push(pushByStorage));
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
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var fileSystemScanner = scope.ServiceProvider.GetRequiredService<IFileSystemScanner>();
        var startTime = DateTime.UtcNow;
        var scanPath = FileSystemPath.Create(parameters.Path ?? "");
        var result = await fileSystemScanner.Scan(scanPath);

        await WriteReport("scan", startTime, GenerateReport(scanPath.Value, result, startTime));
        _jobResultProvider.OnJobCompleted(new JobResult(parameters, result));
    }

    public async Task Verify(VerifyParameters parameters)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var verifier = scope.ServiceProvider.GetRequiredService<IStorageLocationIntegrityVerificationService>();
        var startTime = DateTime.UtcNow;

        var result = await verifier.Execute(parameters.StorageType,
            FileSystemPath.Create(parameters.StoragePath ?? ""),
            FileSystemPath.Create(parameters.VerifyPath ?? ""),
            !string.IsNullOrWhiteSpace(parameters.PathsToSkip)
                ? parameters.PathsToSkip.Split(',', StringSplitOptions.TrimEntries).Select(FileSystemPath.Create)
                : new List<FileSystemPath>());

        // Generate as many thumbnails as possible. Any that fail to generate will be lazy-generated
        _ = Task.Run(async () =>
        {
            await Parallel.ForEachAsync(result.Result.New,
                async (newFile, token) => { await _thumbnailProvider.GetThumbnail(newFile.Path); });
        });

        var report = result.GenerateReport(parameters.StorageType.ToString(), parameters.StoragePath!,
            parameters.VerifyPath!, parameters.PathsToSkip!, startTime);
        await WriteReport("verify", startTime, report);

        _jobResultProvider.OnJobCompleted(new JobResult(parameters, result));
    }

    public async Task Push(PushParameters parameters)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var filePusher = scope.ServiceProvider.GetRequiredService<IFilePusher>();
        var result = await filePusher.PushFilesInDir(StorageLocationType.Local,
            FileSystemPath.Create(parameters.SourceRootPath ?? ""),
            FileSystemPath.Create(parameters.SourcePushPath ?? ""),
            parameters.DestinationType,
            FileSystemPath.Create(parameters.DestinationPath ?? ""));
        _jobResultProvider.OnJobCompleted(new JobResult(parameters, result));
    }

    public async Task Push(PushByStorageParameters parameters)
    {
        var result = await _filePusher.PushFilesInStorage(parameters.StorageLocationId, parameters.PathPrefixFilter,
            parameters.UploadLimitMb);
        _jobResultProvider.OnJobCompleted(new JobResult(parameters, result));
    }

    public async Task Sort(SortParameters parameters)
    {
        try
        {
            var location =
                (await _storageLocationRepo.GetAll()).FirstOrDefault(x => x.Type == StorageLocationType.Local);
            if (location == null) throw new NoLocalStorageException("No Local storage for sorting");

            var sortSourcePath =
                FileSystemPath.Create($"{location.Path.Value}{Path.AltDirectorySeparatorChar}{DefaultSortDir}",
                    OperatingSystem.IsWindows());

            var result = await _fileSorter.Sort(sortSourcePath, location.Path);
            _jobResultProvider.OnJobCompleted(new JobResult(parameters, result));
        }
        catch (SortPathSameAsStoragePathException e)
        {
            _logger.LogError(e.Message);
            _jobResultProvider.OnJobFailed(e.Message);
        }
    }

    private bool IsJobRunning(string jobName)
    {
        var monitoringApi = JobStorage.Current.GetMonitoringApi();
        var processingJobs = monitoringApi.ProcessingJobs(0, int.MaxValue);

        return processingJobs.Any(job => job.Value.Job.Method.Name == jobName);
    }

    private IReadOnlyList<FileResultList> GenerateReport(string path, FileSystemScannerResult result,
        DateTime startTime)
    {
        var newFiles = result.NewFiles.Select(x => (string[])[x.Item1.Value]).ToImmutableList();
        var duration = DateTime.UtcNow.Subtract(startTime);

        return new List<FileResultList>()
        {
            new("Scan Path", new List<string[]>() { new[] { path } }.ToImmutableList(), false),
            new("Duration", new List<string[]>() { new[] { $"{duration.TotalMinutes} mins" } }.ToImmutableList(),
                false),
            new("New", newFiles, true)
        }.AsReadOnly();
    }

    private async Task WriteReport(string reportName, DateTime startTime, IReadOnlyList<FileResultList> report)
    {
        var dir = Directory.CreateDirectory(_jobConfiguration.ReportPath);
        var filePath = Path.Combine(dir.FullName, $"{startTime:yyyy-MM-dd__HH-mm-ss}_{reportName}.json");
        await using var fileStream = File.CreateText(filePath);
        await fileStream.WriteAsync(JsonSerializer.Serialize(report));
    }
}