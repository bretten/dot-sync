using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Logger;

/// <summary>
/// Logger that reports progress with <see cref="IJobProgressReporter"/>
/// </summary>
/// <param name="name">Category name</param>
/// <param name="config">Logger config</param>
/// <param name="progressReporter"><see cref="IJobProgressReporter"/></param>
public sealed class JobProgressLogger(
    string name,
    JobProgressLoggerConfiguration config,
    IJobProgressReporter progressReporter,
    IExternalScopeProvider scopeProvider) : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var jobId = ExtractJobId(state);
        if (jobId == null) return;

        progressReporter.ReportLog(this, jobId.Value, $"{jobId:D} {formatter(state, exception)}");
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    private Guid? ExtractJobId<TState>(TState state)
    {
        Guid? jobId = null;

        scopeProvider.ForEachScope((scope, _) =>
        {
            if (scope is not IEnumerable<KeyValuePair<string, object>> pairs) return;
            foreach (var pair in pairs)
            {
                if (pair.Key != config.JobExecutionContextKey) continue;
                jobId = (Guid)pair.Value;
            }
        }, state);
        return jobId;
    }
}