using System.Collections.Concurrent;
using com.brettnamba.DotSync.Common.Application.Jobs;
using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.Common.Application.Jobs.Events;
using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.Common.Application.Jobs.Progress;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.Common.Infrastructure.Jobs.Execution;

/// <summary>
/// Hangfire implementation of the job manager
/// </summary>
public sealed class HangfireJobManager : IJobManager, IDisposable
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IJobProgressReporter _progressReporter;
    private readonly ILogger<HangfireJobManager> _logger;
    private readonly List<IJob> _jobs = new();
    private readonly ConcurrentDictionary<Guid, List<string>> _jobLogs = new();
    private readonly ConcurrentDictionary<Guid, ProgressPercent> _jobProgress = new();
    private readonly ConcurrentDictionary<Guid, IJobOutput> _jobOutput = new();

    public HangfireJobManager(IServiceScopeFactory serviceScopeFactory, IJobProgressReporter progressReporter,
        ILogger<HangfireJobManager> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _progressReporter = progressReporter;
        _logger = logger;
        _progressReporter.LogReported += HandleLogReported;
        _progressReporter.PercentReported += HandlePercentReported;
    }

    /// <inheritdoc />
    public IReadOnlyList<IJob> Jobs => _jobs;

    /// <inheritdoc />
    public IReadOnlyList<string> GetJobLogs(Guid jobId)
    {
        _jobLogs.TryGetValue(jobId, out var result);
        return result ?? [];
    }

    /// <inheritdoc />
    public ProgressPercent? CheckJobProgress(Guid jobId)
    {
        _jobProgress.TryGetValue(jobId, out var result);
        return result;
    }

    /// <inheritdoc />
    public IJobOutput? GetJobOutput(Guid jobId)
    {
        _jobOutput.TryGetValue(jobId, out var result);
        return result;
    }

    public event EventHandler<IJob>? JobCreated;
    public event EventHandler<JobCompletedArgs>? JobCompleted;
    public event EventHandler<JobFailedArgs>? JobFailed;

    /// <inheritdoc />
    public async Task RunJob<T>(T jobParameters) where T : IJobParameters
    {
        var handlerType = typeof(IJobRunner<>).MakeGenericType(typeof(T));
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<JobExecutionContext>();

        var handler = (IJobRunner<T>)scope.ServiceProvider.GetRequiredService(handlerType);

        var job = CreateNewJob(context, jobParameters);
        JobCreated?.Invoke(this, job);
        try
        {
            _logger.LogInformation($"Executing job {jobParameters.Type} of type {job.Type}");
            var result = await handler.Execute(job);
            _jobOutput.TryAdd(job.Id, result);

            JobCompleted?.Invoke(this, new JobCompletedArgs() { Job = job, Result = result });
        }
        catch (Exception e)
        {
            _jobOutput.TryAdd(job.Id, JobOutput.Failed(job, e));
            JobFailed?.Invoke(this, new JobFailedArgs() { Job = job, Exception = e });
            throw;
        }
    }

    private Job<T> CreateNewJob<T>(JobExecutionContext context, T jobParameters) where T : IJobParameters
    {
        var job = new Job<T>(context.Id, jobParameters.Type, JobState.Queued, jobParameters);
        _jobs.Add(job);
        _jobLogs.TryAdd(job.Id, new List<string>());
        _jobProgress.TryAdd(job.Id, new ProgressPercent(job.Id, 0, 1));
        return job;
    }

    private void HandleLogReported(object? sender, ProgressLog e)
    {
        _jobLogs[e.JobId].Add(e.Message);
    }

    private void HandlePercentReported(object? sender, ProgressPercent e)
    {
        _jobProgress.TryUpdate(e.JobId, e, _jobProgress[e.JobId]);
    }

    public void Dispose()
    {
        _progressReporter.LogReported -= HandleLogReported;
        _progressReporter.PercentReported -= HandlePercentReported;
    }
}