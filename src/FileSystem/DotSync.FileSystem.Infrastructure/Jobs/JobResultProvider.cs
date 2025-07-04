using com.brettnamba.DotSync.FileSystem.Application.Jobs;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs;

/// <inheritdoc />
public sealed class JobResultProvider : IJobResultProvider
{
    /// <inheritdoc />
    public event EventHandler<JobResult>? JobCompleted;

    /// <inheritdoc />
    public void OnJobCompleted(JobResult result)
    {
        JobCompleted?.Invoke(this, result);
    }
}