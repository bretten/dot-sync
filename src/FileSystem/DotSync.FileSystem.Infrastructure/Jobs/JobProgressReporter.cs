using com.brettnamba.DotSync.FileSystem.Application.Jobs;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs;

/// <inheritdoc />
public sealed class JobProgressReporter : IJobProgressReporter
{
    /// <inheritdoc />
    public event EventHandler<ProgressLog>? LogReported;

    /// <inheritdoc />
    public event EventHandler<ProgressPercent>? PercentReported;

    /// <inheritdoc />
    public void ReportLog(string jobId, string message)
    {
        LogReported?.Invoke(jobId, new ProgressLog(jobId, message));
    }

    /// <inheritdoc />
    public void ReportPercent(object sender, Guid jobId, double currentProgressUnits, double totalProgressUnits)
    {
        var progress = new ProgressPercent(jobId, currentProgressUnits, totalProgressUnits);
        PercentReported?.Invoke(sender, progress);
    }
}