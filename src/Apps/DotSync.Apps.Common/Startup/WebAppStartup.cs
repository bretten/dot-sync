using System.Security.Cryptography.X509Certificates;
using com.brettnamba.DotSync.Apps.Common.Hangfire;
using com.brettnamba.DotSync.Common.Application.State;
using com.brettnamba.DotSync.Common.Infrastructure.Aws;
using com.brettnamba.DotSync.FileSystem.Application.Files.Thumbnails;
using com.brettnamba.DotSync.FileSystem.Application.Maintenance;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace com.brettnamba.DotSync.Apps.Common.Startup;

/// <summary>
/// Startup methods
/// </summary>
public static class WebAppStartup
{
    /// <summary>
    /// Configures web server and SSL certs
    /// </summary>
    /// <param name="builder"></param>
#pragma warning disable CA1416
    public static void ConfigureWebServer(this WebApplicationBuilder builder)
    {
        if (OperatingSystem.IsBrowser()) return;
        builder.WebHost.ConfigureKestrel(async void (x) =>
        {
            var secretsProvider = builder.Configuration.GetSecretsProvider();
            var secrets = await secretsProvider.GetSecrets();
            x.ConfigureHttpsDefaults(o =>
            {
                o.ServerCertificate = new X509Certificate2(secrets.SslCertPath, secrets.SslCertPass);
            });
        });
    }
#pragma warning restore CA1416

    /// <summary>
    /// Maps endpoints for the app
    /// </summary>
    public static void MapEndpoints(this WebApplication app)
    {
        // Thumbnail provider
        app.MapGet("/thumbnail", async ([FromQuery] string path, IThumbnailProvider provider) =>
        {
            var thumbnail = await provider.GetThumbnail(FileSystemPath.Create(path));
            return Results.File(thumbnail.Path, contentType: thumbnail.ContentType);
        });
        // File serving
        app.MapGet("/file", async ([FromQuery] string path, IStorageLocationRepository storageLocationRepo) =>
        {
            var filePath = await storageLocationRepo.GetPathInMainStorage(FileSystemPath.Create(path));
            return Results.File(filePath, fileDownloadName: Path.GetFileName(filePath), enableRangeProcessing: true);
        });
    }

    /// <summary>
    /// Migrates the database to the current version
    /// </summary>
    public static void MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        // DB migration
        using var dbContext = scope.ServiceProvider.GetRequiredService<FileSystemsDbContext>();
        dbContext.Database.Migrate();
    }

    /// <summary>
    /// Runs maintenance
    /// </summary>
    public static void RunMaintenance(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var hangfire = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        var runMaintenance = app.Configuration.GetValue<bool>("Maintenance:Active");
        if (runMaintenance)
        {
            var backfiller = scope.ServiceProvider.GetRequiredService<IFileBackfiller>();
            hangfire.Enqueue(() => backfiller.BackfillThumbnails());
            //hangfire.Enqueue(() => backfiller.BackfillSyncedFiles());
            hangfire.Enqueue(() => backfiller.BackfillIncorrectDates());

            // var statusChecker = scope.ServiceProvider.GetRequiredService<IFileStatusChecker>();
            // hangfire.Enqueue(() =>
            //     statusChecker.CheckFileStatus(
            //         app.Configuration.GetValue<string>("Maintenance:FileStatusCheckerPath")!));
        }
    }

    /// <summary>
    /// Sets up state management
    /// </summary>
    public static void SetupStateManagement(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var hangfire = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        var state = scope.ServiceProvider.GetRequiredService<IEphemeralState>();
        hangfire.Enqueue(() => state.GetAllFileDirectories());
    }

    /// <summary>
    /// Performs setup for Hangfire
    /// </summary>
    public static void SetupHangfire(this WebApplication app)
    {
        var authFilters = new List<IDashboardAuthorizationFilter>();
        if (app.Environment.IsDevelopment())
        {
            authFilters.Add(new LocalRequestsOnlyAuthorizationFilter());
        }

        authFilters.Add(new IpAuthorizationFilter(
            new IpAuthorizationFilterOptions(app.Configuration.GetSection("Hangfire:AllowedIps").Get<string[]>()!)));

        app.UseHangfireDashboard(options: new DashboardOptions
        {
            Authorization = authFilters,
            IgnoreAntiforgeryToken = app.Configuration.GetValue<bool?>("Hangfire:IgnoreAntiforgeryToken") ?? false
        });
    }
}