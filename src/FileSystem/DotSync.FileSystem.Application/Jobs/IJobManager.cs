namespace com.brettnamba.DotSync.FileSystem.Application.Jobs;

/// <summary>
/// Manages jobs that are run by the system
/// </summary>
public interface IJobManager
{
    IReadOnlyList<IJob> Jobs { get; }

    /// <summary>
    /// Invoked when a job is created
    /// </summary>
    event EventHandler<IJob> JobCreated;

    /// <summary>
    /// Invoked when a job is finished
    /// </summary>
    event EventHandler<IJob>? JobCompleted;

    /// <summary>
    /// Invoked when a job fails
    /// </summary>
    event EventHandler<IJob>? JobFailed;

    /// <summary>
    /// Runs the specified job
    /// </summary>
    /// <param name="jobParameters">The job parameters</param>
    /// <typeparam name="T">The type of job</typeparam>
    /// <returns>Completed task</returns>
    Task RunJob<T>(T jobParameters) where T : IJobParameters;
}