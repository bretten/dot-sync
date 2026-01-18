using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.Common.Infrastructure.Jobs.Handlers;

public abstract class BaseJobRunner<T> : IJobRunner<T> where T : IJobParameters
{
    /// <summary>
    /// The execution context of the job
    /// </summary>
    protected readonly JobExecutionContext Context;

    private readonly IClock _clock;
    private readonly JobConfiguration _jobConfiguration;
    private readonly ILogger<BaseJobRunner<T>> _logger;

    protected BaseJobRunner(JobExecutionContext context, IClock clock, JobConfiguration jobConfiguration,
        ILogger<BaseJobRunner<T>> logger)
    {
        Context = context;
        _clock = clock;
        _jobConfiguration = jobConfiguration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IJobOutput> Execute(IJob<T> job)
    {
        _logger.LogInformation($"Executing job {job.Id:D} (Context: {Context.Id:D})");

        try
        {
            job.SetInProgress(_clock.GetUtcNow());
            var result = await RunJob(job);
            //await WriteReport(job.Parameters.JobId, startTime, GenerateReport(scanPath.Value, result, startTime));
            job.SetDone(_clock.GetUtcNow());
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            job.SetError(_clock.GetUtcNow());
            throw;
        }
    }

    /// <summary>
    /// Derived classes should implement the job logic here
    /// </summary>
    /// <param name="job">The job to run</param>
    protected abstract Task<IJobOutput> RunJob(IJob<T> job);
}