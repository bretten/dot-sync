using com.brettnamba.DotSync.FileSystem.Application.Jobs;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs;

/// <inheritdoc />
public sealed class JobProgressReporter : IJobProgressReporter
{
    /// <inheritdoc />
    public event EventHandler<string>? ProgressReported;

    /// <inheritdoc />
    public void ReportProgress(string progress)
    {
        ProgressReported?.Invoke(this, progress);
    }
}