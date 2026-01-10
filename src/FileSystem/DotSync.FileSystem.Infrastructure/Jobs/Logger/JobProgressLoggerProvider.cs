using System.Collections.Concurrent;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Logger;

[ProviderAlias("JobProgress")]
public sealed class JobProgressLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly IDisposable? _onChangeToken;
    private JobProgressLoggerConfiguration _config;
    private readonly IJobProgressReporter _progressReporter;
    private IExternalScopeProvider? _scopeProvider;

    private readonly ConcurrentDictionary<string, JobProgressLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public JobProgressLoggerProvider(
        IOptionsMonitor<JobProgressLoggerConfiguration> config, IJobProgressReporter progressReporter)
    {
        _config = config.CurrentValue;

        // If the configuration changes (at startup), update the config
        _onChangeToken = config.OnChange(updatedConfig => _config = updatedConfig);
        _progressReporter = progressReporter;
    }

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName,
            name => new JobProgressLogger(name, _config, _progressReporter, _scopeProvider!));

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }
}