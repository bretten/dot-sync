namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Logger;

/// <summary>
/// Configuration for <see cref="JobProgressLogger"/>
/// </summary>
public sealed class JobProgressLoggerConfiguration
{
    /// <summary>
    /// Services that should have progress reported
    /// </summary>
    private readonly Dictionary<string, Type> _jobProgressServices =
        new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

    public void AddService(params Type[] serviceTypes)
    {
        foreach (var type in serviceTypes)
        {
            var key = type.FullName!;
            _jobProgressServices.TryAdd(key, type);
        }
    }

    public bool IsServiceConfigured(string serviceName)
    {
        return _jobProgressServices.ContainsKey(serviceName);
    }
}