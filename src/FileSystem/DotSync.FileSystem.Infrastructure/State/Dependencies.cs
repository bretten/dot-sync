using com.brettnamba.DotSync.Common.Application.State;
using Microsoft.Extensions.DependencyInjection;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.State;

/// <summary>
/// Dependencies for state management
/// </summary>
public static class Dependencies
{
    /// <summary>
    /// Adds depenencies for state managaement
    /// </summary>
    public static void AddStateManagement(this IServiceCollection services)
    {
        // State
        services.AddMemoryCache();
        services.AddSingleton<IEphemeralState, MemoryCacheEphemeralState>();
    }
}