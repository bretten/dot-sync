namespace com.brettnamba.DotSync.Common.Infrastructure.Jobs.Logger;

/// <summary>
/// Configuration for <see cref="JobProgressLogger"/>
/// </summary>
public sealed class JobProgressLoggerConfiguration
{
    public string JobExecutionContextKey { get; private set; } = null!;

    public void SetJobExecutionContextKey(string jobExecutionContextKey)
    {
        JobExecutionContextKey = jobExecutionContextKey;
    }
}