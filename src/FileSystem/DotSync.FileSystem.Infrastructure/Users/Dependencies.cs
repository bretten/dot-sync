using com.brettnamba.DotSync.FileSystem.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Users;

/// <summary>
/// Dependencies for user services
/// </summary>
public static class Dependencies
{
    /// <summary>
    /// Adds any dependencies for user services
    /// </summary>
    public static void AddUserServices(this IServiceCollection services)
    {
        // State
        services.AddSingleton<IUserSettings, InMemoryUserSettings>();
    }
}