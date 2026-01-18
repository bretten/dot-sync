using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.Common.Application.Jobs.Progress;

namespace com.brettnamba.DotSync.Common.Infrastructure.Jobs.Progress;

/// <inheritdoc />
public sealed class JobProgressReporter : IJobProgressReporter
{
    /// <inheritdoc />
    public event EventHandler<ProgressLog>? LogReported;

    /// <inheritdoc />
    public event EventHandler<ProgressPercent>? PercentReported;

    /// <inheritdoc />
    public void ReportLog(object sender, Guid jobId, string message)
    {
        LogReported?.Invoke(sender, new ProgressLog(jobId, message));
    }

    /// <inheritdoc />
    public void ReportPercent(object sender, Guid jobId, double currentProgressUnits, double totalProgressUnits)
    {
        var progress = new ProgressPercent(jobId, currentProgressUnits, totalProgressUnits);
        PercentReported?.Invoke(sender, progress);
    }
}