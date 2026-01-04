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
    /// <typeparam name="T">The type of job</typeparam>
    /// <returns>Completed task</returns>
    Task RunJob<T>(T jobParameters) where T : IJobParameters;
}