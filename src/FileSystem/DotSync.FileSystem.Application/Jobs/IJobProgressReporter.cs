namespace com.brettnamba.DotSync.FileSystem.Application.Jobs;

/// <summary>
/// Reports on job progress
/// </summary>
public interface IJobProgressReporter
{
    /// <summary>
    /// Invoked on a progress update
    /// </summary>
    event EventHandler<ProgressLog> LogReported;

    /// <summary>
    /// Reports a log message from the job to subscribers
    /// </summary>
    void ReportLog(object sender, Guid jobId, string message);

    /// <summary>
    /// Invoked when the progress (as a percentage) of a job is updated
    /// </summary>
    event EventHandler<ProgressPercent> PercentReported;

    /// <summary>
    /// Reports the percent progress of a job to subscribers
    /// </summary>
    /// <param name="sender">The calling object</param>
    /// <param name="jobId">Job ID</param>
    /// <param name="currentProgressUnits">Current progress (arbitrary units)</param>
    /// <param name="totalProgressUnits">Total progress (arbitrary units)</param>
    void ReportPercent(object sender, Guid jobId, double currentProgressUnits, double totalProgressUnits);
}