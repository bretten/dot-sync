namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Logger;

/// <summary>
/// Configuration for <see cref="JobProgressLogger"/>
/// </summary>
public sealed class JobProgressLoggerConfiguration
{
    /// <summary>
    /// Services that should have progress reported
    /// </summary>
    private readonly Dictionary<string, JobService> _jobProgressServices = new(StringComparer.OrdinalIgnoreCase);

    public void AddService(params JobService[] jobServices)
    {
        foreach (var jobService in jobServices)
        {
            _jobProgressServices.TryAdd(jobService.ServiceName, jobService);
        }
    }

    public JobService? GetJobService(string serviceName)
    {
        return _jobProgressServices.GetValueOrDefault(serviceName);
    }

    public sealed record JobService(string ServiceName, string JobId);
}