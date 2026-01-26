using com.brettnamba.DotSync.FileSystem.Domain.FileOrganization.Services;
using Microsoft.Extensions.DependencyInjection;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileOrganization;

/// <summary>
/// Dependencies for file organization
/// </summary>
public static class Dependencies
{
    /// <summary>
    /// Adds dependencies for file organization
    /// </summary>
    public static void AddFileOrganization(this IServiceCollection services)
    {
        services.AddScoped<IFileSorter, LocalFileSystemByDateFileSorter>();
    }
}