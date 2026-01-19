using com.brettnamba.DotSync.Common.Application.Jobs.Execution;

namespace com.brettnamba.DotSync.Common.Application.Jobs.Contracts;

public interface IJobOutput
{
    /// <summary>
    /// The job that was executed
    /// </summary>
    public IJob Job { get; }

    /// <summary>
    /// Result data from the job execution
    /// </summary>
    public JobResults JobResults { get; }

    /// <summary>
    /// If the job threw an exception, will be non-null
    /// </summary>
    public Exception? Exception { get; }
}