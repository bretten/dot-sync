using Amazon.S3;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems;

/// <summary>
/// Dependencies for file systems
/// </summary>
public static class Dependencies
{
    /// <summary>
    /// Adds dependencies for file system domain services
    /// </summary>
    public static async Task AddFileSystems(this IServiceCollection services, IConfiguration configuration)
    {
        // DB and EF Core
        const string migrationsTable = "__EFMigrationsHistory";
        const string fileSystemsSchema = Constants.Schema;

        // Determine the connection string
        var cs = configuration.GetConnectionString("FileSystems");
        if (string.IsNullOrWhiteSpace(cs)) // Gets the connection string from AWS secrets provider
        {
            var secretsProvider = Common.Infrastructure.Aws.Dependencies.GetSecretsProvider(configuration);
            var secrets = await secretsProvider.GetSecrets();
            cs = secrets.ConnectionString;
        }

        // Database connection
        var dataSource = new NpgsqlDataSourceBuilder(cs).Build();
        services.AddSingleton(dataSource);
        services.AddDbContext<FileSystemsDbContext>(optionsBuilder =>
        {
            optionsBuilder.UseNpgsql(cs,
                b =>
                {
                    b.EnableRetryOnFailure(5, TimeSpan.FromSeconds(20), null);
                    b.MigrationsHistoryTable(migrationsTable, fileSystemsSchema);
                });
        }, optionsLifetime: ServiceLifetime.Singleton); // Options lifetime needs to be singleton because DbContextFactory is
        services.AddDbContextFactory<FileSystemsDbContext>(optionsBuilder => optionsBuilder.UseNpgsql(cs,
            b => b.MigrationsHistoryTable(migrationsTable, fileSystemsSchema))
        );

        // Repos
        services.AddScoped<IFileRepository, EntityFrameworkCoreFileRepository>();
        services.AddScoped<IStorageLocationRepository, EntityFrameworkCoreStorageLocationRepository>();

        // File metadata
        if (!OperatingSystem.IsWindows())
        {
            services.AddTransient<IFileMetadataReader, CrossPlatformFileMetadataReader>();
        }
        else
        {
            //services.AddTransient<IFileMetadataReader, WindowsFileMetadataReader>();
            services.AddTransient<IFileMetadataReader, CrossPlatformFileMetadataReader>();
        }

        // File transfer
        services.AddScoped<IFileSystemScanner, LocalFileSystemScanner>();
        services.AddScoped<IFileCopier, AmazonS3FileCopier>(sp =>
        {
            var storageClass = S3StorageClass.FindValue(configuration["Aws:S3:StorageClass"]) ??
                               throw new ArgumentException($"Storage class not defined");

            return new AmazonS3FileCopier(sp.GetRequiredService<IFileChecksumGenerator>(),
                sp.GetRequiredService<IAmazonS3>(), storageClass, sp.GetRequiredService<ILogger<AmazonS3FileCopier>>());
        });
    }
}