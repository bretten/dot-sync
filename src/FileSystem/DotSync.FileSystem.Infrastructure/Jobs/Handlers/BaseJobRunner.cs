using System.Text.Json;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Handlers;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Handlers;

public abstract class BaseJobRunner<T> : IJobRunner<T> where T : IJobParameters
{
    /// <summary>
    /// The execution context of the job
    /// </summary>
    protected readonly JobExecutionContext Context;

    private readonly IClock _clock;
    private readonly JobConfiguration _jobConfiguration;
    private readonly IJobResultProvider _jobResultProvider;
    private readonly ILogger<BaseJobRunner<T>> _logger;

    protected BaseJobRunner(JobExecutionContext context, IClock clock, JobConfiguration jobConfiguration,
        IJobResultProvider jobResultProvider, ILogger<BaseJobRunner<T>> logger)
    {
        Context = context;
        _clock = clock;
        _jobConfiguration = jobConfiguration;
        _jobResultProvider = jobResultProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Execute(Job<T> job)
    {
        _logger.LogInformation($"Executing job {job.Id:D} (Context: {Context.Id:D})");
        var startTime = _clock.GetUtcNow();

        try
        {
            job.SetInProgress();
            var result = await RunJob(job);
            _jobResultProvider.OnJobCompleted(result);
            //await WriteReport(job.Parameters.JobId, startTime, GenerateReport(scanPath.Value, result, startTime));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            _jobResultProvider.OnJobFailed(e.Message);
            job.SetError();
        }

        job.SetDone();
    }

    /// <summary>
    /// Derived classes should implement the job logic here
    /// </summary>
    /// <param name="job">The job to run</param>
    protected abstract Task<JobResult> RunJob(Job<T> job);

    private async Task WriteReport(string reportName, DateTimeOffset startTime, IReadOnlyList<FileResultList> report)
    {
        var dir = Directory.CreateDirectory(_jobConfiguration.ReportPath);
        var filePath = Path.Combine(dir.FullName, $"{startTime.ToLocalTime():yyyy-MM-dd__HH-mm-ss}_{reportName}.json");
        await using var fileStream = File.CreateText(filePath);
        await fileStream.WriteAsync(JsonSerializer.Serialize(report));
    }
}