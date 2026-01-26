using System.Reflection;
using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.Common.Infrastructure.Configuration;
using com.brettnamba.DotSync.Common.Infrastructure.Jobs.Execution;
using com.brettnamba.DotSync.Common.Infrastructure.Jobs.Logger;
using com.brettnamba.DotSync.Common.Infrastructure.Jobs.Progress;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.Common.Infrastructure.Jobs;

/// <summary>
/// Dependencies for jobs
/// </summary>
public static class Dependencies
{
    /// <summary>
    /// Adds job dependencies
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration"><see cref="IConfiguration"/></param>
    /// <param name="assembliesToScan">Assemblies to scan for <see cref="IJobRunner{T}"/></param>
    public static void AddJobs(this IServiceCollection services, IConfiguration configuration,
        List<Assembly> assembliesToScan)
    {
        // Jobs
        services.AddSingleton<JobProgressLoggerConfiguration>();
        services.AddScoped<JobExecutionContext>();
        services.AddSingleton<IJobManager, HangfireJobManager>();
        services.AddSingleton<IJobProgressReporter, JobProgressReporter>();
        services.AddSingleton<IJobValidator, SingleInstanceJobValidator>();
        services.AddSingleton(new JobConfiguration(configuration["JobConfiguration:ReportPath"]!));
        // Register all job runners
        assembliesToScan.SelectMany(x => x.GetTypes())
            .Where(x =>
            {
                var implementsJobRunner = x.GetInterfaces()
                    .Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(IJobRunner<>));
                return implementsJobRunner && !x.IsAbstract && !x.IsInterface;
            })
            .ToList()
            .ForEach(x =>
            {
                var jobType = x.BaseType!.GetGenericArguments()[0];
                var jobRunnerType = typeof(IJobRunner<>).MakeGenericType(jobType);
                services.AddScoped(jobRunnerType, x);
            });
    }

    /// <summary>
    /// Adds job logger that outputs job logs to the UI
    /// </summary>
    /// <param name="builder"><see cref="ILoggingBuilder"/></param>
    public static void AddJobLogging(this ILoggingBuilder builder)
    {
        builder.AddJobProgressLogger(config => { config.SetJobExecutionContextKey(nameof(JobExecutionContext)); });
    }
}