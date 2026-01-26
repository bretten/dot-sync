using com.brettnamba.DotSync.FileSystem.Application.Maintenance;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Maintenance;

/// <summary>
/// Dependencies for maintenance services
/// </summary>
public static class Dependencies
{
    /// <summary>
    /// Adds dependencies for maintenance services
    /// </summary>
    public static void AddMaintenance(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LocalCheckpointFileBackfillerConfiguration>(
            configuration.GetSection(LocalCheckpointFileBackfillerConfiguration.Section));
        services.AddScoped<IFileBackfiller, LocalCheckpointFileBackfiller>();
    }
}