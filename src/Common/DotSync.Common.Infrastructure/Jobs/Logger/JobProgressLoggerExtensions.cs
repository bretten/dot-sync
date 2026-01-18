using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.Common.Infrastructure.Jobs.Logger;

[ExcludeFromCodeCoverage]
public static class JobProgressLoggerExtensions
{
    public static ILoggingBuilder AddJobProgressLogger(this ILoggingBuilder builder)
    {
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, JobProgressLoggerProvider>());

        return builder;
    }

    public static ILoggingBuilder AddJobProgressLogger(this ILoggingBuilder builder,
        Action<JobProgressLoggerConfiguration> configure)
    {
        builder.AddJobProgressLogger();
        builder.Services.Configure(configure);

        return builder;
    }
}