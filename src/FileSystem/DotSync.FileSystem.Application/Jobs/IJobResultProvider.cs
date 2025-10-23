namespace com.brettnamba.DotSync.FileSystem.Application.Jobs;

/// <summary>
/// Provides job results
/// </summary>
public interface IJobResultProvider
{
    /// <summary>
    /// Invoked when a job is finished
    /// </summary>
    event EventHandler<JobResult>? JobCompleted;

    /// <summary>
    /// Handles a completed job
    /// </summary>
    void OnJobCompleted(JobResult result);

    /// <summary>
    /// Invoked when a job fails
    /// </summary>
    event EventHandler<string>? JobFailed;

    /// <summary>
    /// Handles a failed job
    /// </summary>
    void OnJobFailed(string error);
}

/// <summary>
/// Job Result
/// </summary>
/// <param name="JobParameters">The parameters that started the job</param>
/// <param name="Result">The result</param>
public sealed record JobResult(IJobParameters JobParameters, object Result);