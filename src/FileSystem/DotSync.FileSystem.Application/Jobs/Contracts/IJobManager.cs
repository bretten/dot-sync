using com.brettnamba.DotSync.FileSystem.Application.Jobs.Events;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Progress;

namespace com.brettnamba.DotSync.FileSystem.Application.Jobs.Contracts;

/// <summary>
/// Manages jobs that are run by the system
/// </summary>
public interface IJobManager
{
    /// <summary>
    /// All jobs
    /// </summary>
    IReadOnlyList<IJob> Jobs { get; }

    /// <summary>
    /// Returns the progress of the job
    /// </summary>
    /// <param name="jobId">ID of the job</param>
    /// <returns>Progress as a percentage or null if the job could not be found</returns>
    ProgressPercent? CheckJobProgress(Guid jobId);

    /// <summary>
    /// Returns the logs for the job
    /// </summary>
    /// <param name="jobId">ID of the job</param>
    /// <returns>The logs for the job</returns>
    IReadOnlyList<string> GetJobLogs(Guid jobId);

    /// <summary>
    /// Returns the job output
    /// </summary>
    /// <param name="jobId">ID of the job</param>
    /// <returns>The job output or null if the job did/has not finished</returns>
    IJobOutput? GetJobOutput(Guid jobId);

    /// <summary>
    /// Invoked when a job is created
    /// </summary>
    event EventHandler<IJob> JobCreated;

    /// <summary>
    /// Invoked when a job is finished
    /// </summary>
    event EventHandler<JobCompletedArgs>? JobCompleted;

    /// <summary>
    /// Invoked when a job fails
    /// </summary>
    event EventHandler<JobFailedArgs>? JobFailed;

    /// <summary>
    /// Runs the specified job
    /// </summary>
    /// <param name="jobParameters">The job parameters</param>
    /// <typeparam name="T">The type of job</typeparam>
    /// <returns>Completed task</returns>
    Task RunJob<T>(T jobParameters) where T : IJobParameters;
}