using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;
using Microsoft.Extensions.DependencyInjection;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity;

/// <summary>
/// Dependencies for file integrity
/// </summary>
public static class Dependencies
{
    /// <summary>
    /// Adds dependencies for file integrity
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    public static void AddFileIntegrity(this IServiceCollection services)
    {
        services.AddScoped<IFileIntegrityVerifierFactory, FileIntegrityVerifierFactory>();
        services.AddScoped<LocalFileSystemFileIntegrityVerifier>();
        services.AddScoped<AmazonS3FileIntegrityVerifier>();
        services.AddTransient<IFileChecksumGenerator, Sha256FileChecksumGenerator>();
    }
}