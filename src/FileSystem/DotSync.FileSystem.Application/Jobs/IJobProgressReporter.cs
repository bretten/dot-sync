namespace com.brettnamba.DotSync.FileSystem.Application.Jobs;

/// <summary>
/// Reports on job progress
/// </summary>
public interface IJobProgressReporter
{
    /// <summary>
    /// Invoked on a progress update
    /// </summary>
    event ProgressReportedHandler ProgressReported;

    /// <summary>
    /// Handles a progress update
    /// </summary>
    void ReportProgress(string jobId, string progress);
}