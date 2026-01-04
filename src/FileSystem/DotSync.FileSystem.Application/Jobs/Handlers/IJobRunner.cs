namespace com.brettnamba.DotSync.FileSystem.Application.Jobs.Handlers;

/// <summary>
/// Runs a Job
/// </summary>
/// <typeparam name="T">Job parameter type</typeparam>
public interface IJobRunner<T> where T : IJobParameters
{
    /// <summary>
    /// Runs a job
    /// </summary>
    /// <param name="job">The job to run</param>
    /// <returns>Completed Task</returns>
    public Task Execute(Job<T> job);
}