using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Application.Configuration;
using com.brettnamba.DotSync.FileSystem.Application.Files.Indexing;
using com.brettnamba.DotSync.FileSystem.Application.Files.Thumbnails;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Contracts;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Execution;
using com.brettnamba.DotSync.FileSystem.Application.Maintenance;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Application.State;
using com.brettnamba.DotSync.FileSystem.Application.Storage;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileOrganization.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Configuration;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Files.Indexing;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Files.Thumbnails;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Execution;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Handlers;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Logger;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Progress;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Maintenance;
using com.brettnamba.DotSync.FileSystem.Infrastructure.State;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Storage;
using com.brettnamba.DotSync.FileSystem.WebApp.Components;
using com.brettnamba.DotSync.FileSystem.WebApp.Components.Jobs;
using com.brettnamba.DotSync.FileSystem.WebApp.Hangfire;
using com.brettnamba.DotSync.FileSystem.WebApp.Startup;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Npgsql;
using Constants = com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore.Constants;

var builder = WebApplication.CreateBuilder(args);

// Core
builder.Services.AddTransient<IClock, Clock>();

builder.Services.AddMemoryCache();

builder.Services.AddOidc(builder.Configuration);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMemoryStorage(new MemoryStorageOptions()
    {
        FetchNextJobTimeout = TimeSpan.FromHours(24)
    }));
builder.Services.AddHangfireServer();

builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Configuration.AddEnvironmentVariables();

// Jobs
builder.Services.AddSingleton<JobProgressLoggerConfiguration>();
builder.Services.AddScoped<JobExecutionContext>();
builder.Logging.AddJobProgressLogger(config => { config.SetJobExecutionContextKey(nameof(JobExecutionContext)); });
// Register all job runners
new List<Assembly>()
    {
        typeof(BaseJobRunner<>).Assembly
    }.SelectMany(x => x.GetTypes())
    .Where(x =>
    {
        var implementsJobRunner = x.GetInterfaces()
            .Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(IJobRunner<>));
        return implementsJobRunner && !x.IsAbstract && !x.IsInterface;
    })
    .ToList()
    .ForEach(x =>
    {
        var jobType = x.BaseType!.GetGenericArguments()[0];
        var jobRunnerType = typeof(IJobRunner<>).MakeGenericType(jobType);
        builder.Services.AddScoped(jobRunnerType, x);
    });

// DB and EF Core
const string migrationsTable = "__EFMigrationsHistory";
const string fileSystemsSchema = Constants.Schema;

var cs = builder.Configuration.GetConnectionString("FileSystems");
if (string.IsNullOrWhiteSpace(cs))
{
    var secretsProvider = GetSecretsProvider(builder.Configuration);
    var secrets = await secretsProvider.GetSecrets();
    cs = secrets.ConnectionString;
}

var dataSource = new NpgsqlDataSourceBuilder(cs).Build();
builder.Services.AddSingleton(dataSource);
builder.Services.AddDbContext<FileSystemsDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(cs,
        b => b.MigrationsHistoryTable(migrationsTable, fileSystemsSchema));
}, optionsLifetime: ServiceLifetime.Singleton); // Options lifetime needs to be singleton because DbContextFactory is
builder.Services.AddDbContextFactory<FileSystemsDbContext>(optionsBuilder => optionsBuilder.UseNpgsql(cs,
    b => b.MigrationsHistoryTable(migrationsTable, fileSystemsSchema))
);
builder.Services.AddTransient<IFileRepository, EntityFrameworkCoreFileRepository>();

// EF-aware IAsyncQueryExecutor
builder.Services.AddQuickGridEntityFrameworkAdapter();

builder.Services.AddTransient<IStorageLocationRepository, EntityFrameworkCoreStorageLocationRepository>();

// File organization
builder.Services.AddTransient<IFileSorter, LocalFileSystemByDateFileSorter>();

// File metadata
if (!OperatingSystem.IsWindows())
{
    builder.Services.AddTransient<IFileMetadataReader, CrossPlatformFileMetadataReader>();
}
else
{
    //builder.Services.AddTransient<IFileMetadataReader, WindowsFileMetadataReader>();
    builder.Services.AddTransient<IFileMetadataReader, CrossPlatformFileMetadataReader>();
}

// File integrity
builder.Services.AddScoped<IFileIntegrityVerifierFactory, FileIntegrityVerifierFactory>();
builder.Services.AddScoped<LocalFileSystemFileIntegrityVerifier>();
builder.Services.AddScoped<AmazonS3FileIntegrityVerifier>();
builder.Services.AddTransient<IFileChecksumGenerator, Sha256FileChecksumGenerator>();

// File transfer
builder.Services.AddScoped<IAmazonS3>(sp =>
{
    if (!string.IsNullOrWhiteSpace(builder.Configuration["Aws:AccessKey"]))
    {
        var awsAccessKeyId = builder.Configuration["Aws:AccessKey"];
        var awsSecretAccessKey = builder.Configuration["Aws:SecretAccessKey"];
        var region = builder.Configuration["Aws:Region"];
        return new AmazonS3Client(new BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey),
            RegionEndpoint.GetBySystemName(region));
    }
    else
    {
        var chain = new CredentialProfileStoreChain();
        AWSConfigs.AWSProfileName = "roles_anywhere";
        if (!chain.TryGetAWSCredentials("roles_anywhere", out var credentials))
        {
            throw new Exception("Missing AWS credentials profile");
        }

        return new AmazonS3Client(credentials);
    }
});

builder.Services.AddScoped<IFileSystemScanner, LocalFileSystemScanner>();
builder.Services.AddScoped<IFileCopier, AmazonS3FileCopier>(sp =>
{
    var storageClass = S3StorageClass.FindValue(builder.Configuration["Aws:S3:StorageClass"]) ??
                       throw new ArgumentException($"Storage class not defined");

    return new AmazonS3FileCopier(sp.GetRequiredService<IFileChecksumGenerator>(),
        sp.GetRequiredService<IAmazonS3>(), storageClass, sp.GetRequiredService<ILogger<AmazonS3FileCopier>>());
});

builder.Services.AddScoped<IFilePusher, FilePusher>();
builder.Services.AddSingleton<IJobManager, HangfireJobManager>();
builder.Services.AddTransient<JobComponent>();
builder.Services.AddSingleton<IJobProgressReporter, JobProgressReporter>();
builder.Services.AddSingleton(new JobConfiguration(builder.Configuration["JobConfiguration:ReportPath"]!));

// Storage provider
builder.Services.AddScoped<IMainStorageProvider, MainStorageProvider>();

// Thumbnails
builder.Services.Configure<ThumbnailConfiguration>(
    builder.Configuration.GetSection(ThumbnailConfiguration.Section));
builder.Services.AddScoped<IThumbnailGenerator, MagickThumbnailGenerator>();
builder.Services.AddScoped<IThumbnailProvider, ThumbnailProvider>();

// State
builder.Services.AddSingleton<IEphemeralState, MemoryCacheEphemeralState>();

// File paths
builder.Services.AddSingleton<IFileDirectoryIndexer, FileDirectoryIndexer>();

// Maintenance
builder.Services.Configure<LocalCheckpointFileBackfillerConfiguration>(
    builder.Configuration.GetSection(LocalCheckpointFileBackfillerConfiguration.Section));
builder.Services.AddScoped<IFileBackfiller, LocalCheckpointFileBackfiller>();

if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(async void (x) =>
    {
        var secretsProvider = GetSecretsProvider(builder.Configuration);
        var secrets = await secretsProvider.GetSecrets();
        x.ConfigureHttpsDefaults(o =>
        {
            o.ServerCertificate = new X509Certificate2(secrets.SslCertPath, secrets.SslCertPass);
        });
    });
}

var app = builder.Build();

using var scope = app.Services.CreateScope();
var hangfire = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
// DB migration
scope.ServiceProvider.GetRequiredService<FileSystemsDbContext>().Database.Migrate();
// Load state
var state = scope.ServiceProvider.GetRequiredService<IEphemeralState>();
hangfire.Enqueue(() => state.GetAllFileDirectories());
// Maintenance
var runMaintenance = app.Configuration.GetValue<bool>("Maintenance:Active");
if (runMaintenance)
{
    var backfiller = scope.ServiceProvider.GetRequiredService<IFileBackfiller>();
    hangfire.Enqueue(() => backfiller.BackfillThumbnails());
    hangfire.Enqueue(() => backfiller.BackfillSyncedFiles());
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAuthenticationEndpoints();

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

// Thumbnail provider
app.MapGet("/thumbnail", async ([FromQuery] string path, IThumbnailProvider provider) =>
{
    var thumbnail = await provider.GetThumbnail(FileSystemPath.Create(path));
    return Results.File(thumbnail.Path, contentType: thumbnail.ContentType);
});
// File serving
app.MapGet("/file", async ([FromQuery] string path, IMainStorageProvider storageProvider) =>
{
    var filePath = await storageProvider.GetFileFullLocalPath(FileSystemPath.Create(path));
    return Results.File(filePath.Value, fileDownloadName: Path.GetFileName(filePath.Value),
        enableRangeProcessing: true);
});

app.Run();

static ISecretsProvider GetSecretsProvider(IConfiguration configuration)
{
    var secretName = configuration["Aws:SecretsManager:SecretName"]!;
    var region = RegionEndpoint.GetBySystemName(configuration["Aws:SecretsManager:Region"]);
    if (!string.IsNullOrWhiteSpace(configuration["Aws:AccessKey"]))
    {
        var awsAccessKeyId = configuration["Aws:AccessKey"];
        var awsSecretAccessKey = configuration["Aws:SecretAccessKey"];

        return new AwsSecretsManagerProvider(secretName, new BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey),
            region);
    }
    else
    {
        var chain = new CredentialProfileStoreChain();
        AWSConfigs.AWSProfileName = "roles_anywhere";
        if (!chain.TryGetAWSCredentials("roles_anywhere", out var credentials))
        {
            throw new Exception("Missing AWS credentials profile");
        }

        return new AwsSecretsManagerProvider(secretName, credentials, region);
    }
}