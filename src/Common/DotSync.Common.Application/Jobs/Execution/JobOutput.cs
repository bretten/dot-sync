using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;

namespace com.brettnamba.DotSync.Common.Application.Jobs.Execution;

public sealed class JobOutput : IJobOutput
{
    /// <inheritdoc/>
    public IJob Job { get; }

    /// <inheritdoc/>
    public JobResults JobResults { get; }

    /// <inheritdoc/>
    public Exception? Exception { get; }

    public JobOutput(IJob job, JobResults jobResults)
    {
        Job = job;
        JobResults = jobResults;
    }

    private JobOutput(IJob job, Exception exception)
    {
        Job = job;
        JobResults = new JobResults(new Dictionary<string, ResultCollection>());
        Exception = exception;
    }

    /// <summary>
    /// Creates a job output that failed
    /// </summary>
    public static JobOutput Failed(IJob job, Exception exception)
    {
        return new JobOutput(job, exception);
    }
}