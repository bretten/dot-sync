using System.Reflection;
using com.brettnamba.DotSync.Apps.Common.Auth;
using com.brettnamba.DotSync.Apps.Common.Components.Jobs;
using com.brettnamba.DotSync.Apps.Common.Components.Pages;
using com.brettnamba.DotSync.Apps.Common.Startup;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Infrastructure.Aws;
using com.brettnamba.DotSync.Common.Infrastructure.Jobs;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileOrganization;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Files;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Handlers;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Maintenance;
using com.brettnamba.DotSync.FileSystem.Infrastructure.State;
using DotSync.Apps.WebApp.Demo.Components;
using DotSync.Apps.WebApp.Demo.Mocks;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

/*
 * Dependencies
 */
// Configuration
builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Configuration.AddEnvironmentVariables();

// Core
builder.Services.AddTransient<IClock, Clock>();

// Open ID
builder.Services.AddOidc(builder.Configuration);

// Razor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MudBlazor
builder.Services.AddMudServices();

// Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMemoryStorage(new MemoryStorageOptions()
    {
        FetchNextJobTimeout = TimeSpan.FromHours(24)
    }));
builder.Services.AddHangfireServer();

// AWS
builder.Services.AddAws(builder.Configuration);

// Jobs
builder.Services.AddJobs(builder.Configuration, new List<Assembly>()
{
    typeof(VerifyJobRunner).Assembly
});
builder.Logging.AddJobLogging();
builder.Services.AddScoped<JobDetailsProvider>();

// File systems
await builder.Services.AddFileSystems(builder.Configuration);

// File integrity
builder.Services.AddFileIntegrity();

// File organization
builder.Services.AddFileOrganization();

// File-related services
builder.Services.AddFileServices(builder.Configuration);

// State management
builder.Services.AddStateManagement();

// Maintenance
builder.Services.AddMaintenance(builder.Configuration);

// Configure web server for dev env
if (!builder.Environment.IsDevelopment())
{
    builder.ConfigureWebServer();
}

/*
 * Override with mock services
 */
builder.Services.AddSingleton<MockS3Storage>();
builder.Services.AddScoped<IFileIntegrityVerifierFactory, MockFileIntegrityVerifierFactory>();
builder.Services.AddScoped<MockAmazonS3FileIntegrityVerifier>();
builder.Services.AddScoped<IFileCopier, MockAmazonS3FileCopier>();

/*
 * Application
 */
var app = builder.Build();

// Migrate the DB to the current version
app.MigrateDatabase();
// Seed the instance with the test state
using (var scope = app.Services.CreateScope())
{
    // Update the fake, internal S3 storage to match the DB
    var mockS3Storage = scope.ServiceProvider.GetRequiredService<MockS3Storage>();
    await mockS3Storage.UpdateState();

    // Seed the database
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<FileSystemsDbContext>>();
    var dbContext = dbContextFactory.CreateDbContext();

    // Make sure there is a default local storage
    var defaultTestStoragePath = StoragePath.Create("/home/app/dot_sync/");
    var mainStorage =
        dbContext.StorageLocations.FirstOrDefault(x =>
            x.Type == StorageLocationType.Local && x.Path == defaultTestStoragePath);
    if (mainStorage == null)
    {
        dbContext.StorageLocations.Add(new StorageLocation(StorageLocationType.Local, defaultTestStoragePath));
        dbContext.SaveChanges();
    }

    // Make sure there is a test S3 bucket
    var s3StorageCount = dbContext.StorageLocations.Count(x => x.Type == StorageLocationType.AmazonS3);
    if (s3StorageCount < 1)
    {
        dbContext.StorageLocations.Add(new StorageLocation(StorageLocationType.AmazonS3,
            StoragePath.Create("/testBucket/")));
        dbContext.SaveChanges();
    }
}

// Load state
app.SetupStateManagement();

// Maintenance
app.RunMaintenance();

// HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// Blazor
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddAdditionalAssemblies(typeof(Home).Assembly)
    .AddInteractiveServerRenderMode();

// Authentication
app.MapAuthenticationEndpoints();

// Hangfire
app.SetupHangfire();

// Map endpoints
app.MapEndpoints();

app.Run();