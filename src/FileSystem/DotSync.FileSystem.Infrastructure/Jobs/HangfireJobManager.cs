using System.Collections.Immutable;
using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Handlers;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs;

/// <summary>
/// Hangfire implementation of the job manager
/// </summary>
public sealed class HangfireJobManager : IJobManager
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<HangfireJobManager> _logger;
    private readonly List<Job<IJobParameters>> _jobs = new List<Job<IJobParameters>>();

    public HangfireJobManager(IServiceScopeFactory serviceScopeFactory, ILogger<HangfireJobManager> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task RunJob<T>(T jobParameters) where T : IJobParameters
    {
        var handlerType = typeof(IJobRunner<>).MakeGenericType(typeof(T));
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<JobExecutionContext>();

        var handler = (IJobRunner<T>)scope.ServiceProvider.GetRequiredService(handlerType);

        var job = new Job<T>(context.Id, jobParameters.JobId, JobState.InProgress, jobParameters);
        _jobs.Add((job as Job<IJobParameters>)!);
        await handler.Execute(job);
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
}