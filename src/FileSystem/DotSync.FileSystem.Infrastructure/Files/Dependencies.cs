using com.brettnamba.DotSync.FileSystem.Application.Configuration;
using com.brettnamba.DotSync.FileSystem.Application.Files.Indexing;
using com.brettnamba.DotSync.FileSystem.Application.Files.Thumbnails;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Files.Indexing;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Files.Thumbnails;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Files;

/// <summary>
/// Dependencies for file related services
/// </summary>
public static class Dependencies
{
    /// <summary>
    /// Adds dependencies for file related services
    /// </summary>
    public static void AddFileServices(this IServiceCollection services, IConfiguration configuration)
    {
        // File paths
        services.AddSingleton<IFileDirectoryIndexer, FileDirectoryIndexer>();

        // Directories
        services.Configure<DirectoryConfiguration>(configuration.GetSection(DirectoryConfiguration.Section));

        // Thumbnails
        services.Configure<ThumbnailConfiguration>(configuration.GetSection(ThumbnailConfiguration.Section));
        services.AddScoped<IThumbnailGenerator, MagickThumbnailGenerator>();
        services.AddScoped<IThumbnailProvider, ThumbnailProvider>();

        // File orchestration
        services.AddScoped<IFilePusher, FilePusher>();
    }
}