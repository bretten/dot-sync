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
    IJobProgressReporter progressReporter) : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var jobService = config.GetJobService(name);
        if (!IsEnabled(logLevel) || jobService == null)
        {
            return;
        }

        progressReporter.ReportProgress(jobService.JobId, $"{formatter(state, exception)}");
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;
}