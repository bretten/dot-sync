using com.brettnamba.DotSync.FileSystem.Application.Jobs;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs;

/// <inheritdoc />
public sealed class JobProgressReporter : IJobProgressReporter
{
    /// <inheritdoc />
    public event ProgressReportedHandler? ProgressReported;

    /// <inheritdoc />
    public void ReportProgress(string jobId, string progress)
    {
        ProgressReported?.Invoke(jobId, progress);
    }
}