using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.Common.Application.Jobs.Extensions;

public static class LoggerExtensions
{
    /// <summary>
    /// Enriches the log with the ID of the job execution context
    ///
    /// Uses log scopes: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line#log-scopes
    /// </summary>
    public static void LogWithScope(this ILogger logger, string message, JobExecutionContext jobExecutionContext)
    {
        using (logger.BeginScope(new List<KeyValuePair<string, object>>()
               {
                   new(nameof(JobExecutionContext), jobExecutionContext.Id)
               }))
        {
            logger.LogInformation(message);
        }
    }
}