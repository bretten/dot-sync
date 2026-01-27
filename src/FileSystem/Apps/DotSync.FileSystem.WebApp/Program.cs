using System.Reflection;
using com.brettnamba.DotSync.Apps.Common.Auth;
using com.brettnamba.DotSync.Apps.Common.Components.Pages;
using com.brettnamba.DotSync.Apps.Common.Startup;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Infrastructure.Aws;
using com.brettnamba.DotSync.Common.Infrastructure.Jobs;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileOrganization;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Files;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Handlers;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Maintenance;
using com.brettnamba.DotSync.FileSystem.Infrastructure.State;
using com.brettnamba.DotSync.FileSystem.WebApp.Components;
using Hangfire;
using Hangfire.MemoryStorage;
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
 * Application
 */
var app = builder.Build();

// Migrate the DB to the current version
app.MigrateDatabase();

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