using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Domain.Tenants;
using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Application.Reporting;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileOrganization.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Repositories;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Logger;
using com.brettnamba.DotSync.FileSystem.Infrastructure.StorageLocations.EntityFrameworkCore;
using com.brettnamba.DotSync.FileSystem.WebApp.Components;
using com.brettnamba.DotSync.FileSystem.WebApp.Components.Jobs;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMemoryStorage());
builder.Services.AddHangfireServer();

builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddSingleton<JobProgressLoggerConfiguration>();
builder.Logging.AddJobProgressLogger(config =>
{
    config.AddService(typeof(IFileSystemScanner), typeof(IFileIntegrityVerifier), typeof(IFileSorter),
        typeof(AmazonS3FileCopier));
});

builder.Services.AddTransient<ITenantContext, TenantContext>(s => new TenantContext(new Tenant("")));
builder.Services.AddTransient<ITenantAware, TenantAware>();
builder.Services.AddTransient<IClock, Clock>();

builder.Services.AddDbContext<FileSystemsDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString($"FileSystems"));
});
builder.Services.AddDbContextFactory<FileSystemsDbContext>(optionsBuilder =>
        optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString($"FileSystems")),
    ServiceLifetime.Scoped
);
builder.Services.AddTransient<IFileRepository, EntityFrameworkCoreFileRepository>();
builder.Services.AddDbContext<StorageLocationsDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString($"StorageLocations"));
});
builder.Services.AddDbContextFactory<StorageLocationsDbContext>(optionsBuilder =>
        optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString($"StorageLocations")),
    ServiceLifetime.Scoped
);
builder.Services.AddTransient<IStorageLocationRepository, EntityFrameworkCoreStorageLocationRepository>();
builder.Services.AddTransient<IFileChecksumGenerator, Sha256FileChecksumGenerator>();
builder.Services.AddTransient<IFileMetadataReader, WindowsFileMetadataReader>();
builder.Services.AddTransient<IFileSorter, LocalFileSystemByDateFileSorter>();
builder.Services.AddTransient<IFileIntegrityVerifierFactory, FileIntegrityVerifierFactory>();
builder.Services.AddTransient<IAmazonS3>(sp =>
{
    var awsAccessKeyId = builder.Configuration[$"AmazonS3:AwsAccessKey"];
    var awsSecretAccessKey = builder.Configuration[$"AmazonS3:AwsSecretAccessKey"];
    var region = builder.Configuration[$"AmazonS3:Region"];
    return new AmazonS3Client(new BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey),
        RegionEndpoint.GetBySystemName(region));
});

builder.Services.AddTransient<IFileSystemScanner, LocalFileSystemScanner>();
builder.Services.AddTransient<IFileCopier, AmazonS3FileCopier>(sp =>
{
    var storageClass = S3StorageClass.FindValue(builder.Configuration[$"AmazonS3:StorageClass"]) ??
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseHangfireDashboard();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();