namespace com.brettnamba.DotSync.FileSystem.Application.Jobs;

/// <summary>
/// Manages jobs that are run by the system
/// </summary>
public interface IJobManager
{
    /// <summary>
    /// Runs the specified job
    /// </summary>
    /// <param name="jobParameters">The job parameters</param>
    /// <returns>Completed task</returns>
    Task RunJob(IJobParameters jobParameters);
}