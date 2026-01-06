using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Events;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Handlers;
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
    private readonly List<IJob> _jobs = new List<IJob>();

    public HangfireJobManager(IServiceScopeFactory serviceScopeFactory, ILogger<HangfireJobManager> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public IReadOnlyList<IJob> Jobs => _jobs;

    public event EventHandler<IJob>? JobCreated;
    public event EventHandler<IJob>? JobCompleted;
    public event EventHandler<JobFailedArgs>? JobFailed;

    /// <inheritdoc />
    public async Task RunJob<T>(T jobParameters) where T : IJobParameters
    {
        var handlerType = typeof(IJobRunner<>).MakeGenericType(typeof(T));
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<JobExecutionContext>();

        var handler = (IJobRunner<T>)scope.ServiceProvider.GetRequiredService(handlerType);

        var job = new Job<T>(context.Id, jobParameters.JobId, JobState.Queued, jobParameters);
        _jobs.Add(job);
        JobCreated?.Invoke(this, job);
        try
        {
            _logger.LogInformation($"Executing job {jobParameters.JobId} of type {job.Type}");
            await handler.Execute(job);
        }
        catch (Exception e)
        {
            JobFailed?.Invoke(this, new JobFailedArgs() { Job = job, Exception = e });
            throw;
        }

        JobCompleted?.Invoke(this, job);
    }
}