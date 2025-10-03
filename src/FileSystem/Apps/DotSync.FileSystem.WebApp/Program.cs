using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Domain.Tenants;
using com.brettnamba.DotSync.FileSystem.Application.Configuration;
using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Application.Reporting;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileOrganization.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Repositories;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Configuration;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Logger;
using com.brettnamba.DotSync.FileSystem.Infrastructure.StorageLocations.EntityFrameworkCore;
using com.brettnamba.DotSync.FileSystem.WebApp.Components;
using com.brettnamba.DotSync.FileSystem.WebApp.Components.Jobs;
using com.brettnamba.DotSync.FileSystem.WebApp.Hangfire;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using Constants = com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

builder.Services.AddSingleton<JobProgressLoggerConfiguration>();
builder.Logging.AddJobProgressLogger(config =>
{
    config.AddService(
        new JobProgressLoggerConfiguration.JobService(typeof(IFileSystemScanner).FullName!, "Scan"),
        new JobProgressLoggerConfiguration.JobService(typeof(IFileIntegrityVerifier).FullName!, "Verify"),
        new JobProgressLoggerConfiguration.JobService(typeof(IFileSorter).FullName!, "Sort"),
        new JobProgressLoggerConfiguration.JobService(typeof(AmazonS3FileCopier).FullName!, "Push")
    );
});

builder.Services.AddTransient<ITenantContext, TenantContext>(s => new TenantContext(new Tenant("")));
builder.Services.AddTransient<ITenantAware, TenantAware>();
builder.Services.AddTransient<IClock, Clock>();


const string migrationsTable = "__EFMigrationsHistory";
const string fileSystemsSchema =
    Constants.Schema;
const string storageLocationSchema = com.brettnamba.DotSync.FileSystem.Infrastructure.StorageLocations.EntityFrameworkCore.Constants.Schema;

var cs = builder.Configuration.GetConnectionString("FileSystems");
if (string.IsNullOrWhiteSpace(cs))
{
    var secretsProvider = GetSecretsProvider(builder.Configuration);
    var secrets = await secretsProvider.GetSecrets();
    cs = secrets.ConnectionString;
}

builder.Services.AddDbContext<FileSystemsDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(cs,
        b => b.MigrationsHistoryTable(migrationsTable, fileSystemsSchema));
});
builder.Services.AddDbContextFactory<FileSystemsDbContext>(
    optionsBuilder => optionsBuilder.UseNpgsql(cs,
        b => b.MigrationsHistoryTable(migrationsTable, fileSystemsSchema)),
    ServiceLifetime.Scoped
);
builder.Services.AddTransient<IFileRepository, EntityFrameworkCoreFileRepository>();
builder.Services.AddDbContext<StorageLocationsDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(cs,
        b => b.MigrationsHistoryTable(migrationsTable, storageLocationSchema));
});
builder.Services.AddDbContextFactory<StorageLocationsDbContext>(
    optionsBuilder => optionsBuilder.UseNpgsql(cs,
        b => b.MigrationsHistoryTable(migrationsTable, storageLocationSchema)),
    ServiceLifetime.Scoped
);
builder.Services.AddTransient<IStorageLocationRepository, EntityFrameworkCoreStorageLocationRepository>();
builder.Services.AddTransient<IFileChecksumGenerator, Sha256FileChecksumGenerator>();
if (!OperatingSystem.IsWindows())
{
    builder.Services.AddTransient<IFileMetadataReader, CrossPlatformFileMetadataReader>();
}
else
{
    //builder.Services.AddTransient<IFileMetadataReader, WindowsFileMetadataReader>();
    builder.Services.AddTransient<IFileMetadataReader, CrossPlatformFileMetadataReader>();
}

builder.Services.AddTransient<IFileSorter, LocalFileSystemByDateFileSorter>();
builder.Services.AddTransient<IFileIntegrityVerifierFactory, FileIntegrityVerifierFactory>();

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

builder.Services.AddTransient<IFileSystemScanner, LocalFileSystemScanner>();
builder.Services.AddTransient<IFileCopier, AmazonS3FileCopier>(sp =>
{
    var storageClass = S3StorageClass.FindValue(builder.Configuration["Aws:S3:StorageClass"]) ??
                       throw new ArgumentException($"Storage class not defined");

    return new AmazonS3FileCopier(sp.GetRequiredService<IFileChecksumGenerator>(),
        sp.GetRequiredService<IAmazonS3>(), storageClass, sp.GetRequiredService<ILogger<AmazonS3FileCopier>>());
});

builder.Services
    .AddTransient<IStorageLocationIntegrityVerificationService, StorageLocationIntegrityVerificationService>();
builder.Services.AddTransient<IIntegrityReporter, HtmlIntegrityReporter>();
builder.Services.AddTransient<IFilePusher, FilePusher>();
builder.Services.AddSingleton<IJobResultProvider, JobResultProvider>();
builder.Services.AddTransient<IJobManager, HangfireJobManager>();
builder.Services.AddTransient<JobComponent>();
builder.Services.AddSingleton<IJobProgressReporter, JobProgressReporter>();
builder.Services.AddSingleton(new JobConfiguration(builder.Configuration["JobConfiguration:ReportPath"]!));

var app = builder.Build();

using var scope = app.Services.CreateScope();
scope.ServiceProvider.GetRequiredService<FileSystemsDbContext>().Database.Migrate();
scope.ServiceProvider.GetRequiredService<StorageLocationsDbContext>().Database.Migrate();

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