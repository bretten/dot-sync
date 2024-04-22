using com.brettnamba.DotSync.Common.Domain.Tenants;
using com.brettnamba.DotSync.FileSystem.Application.Console;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Application.Reporting;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

if (args.Length < 1)
{
    throw new ArgumentException("No file set specified");
}

var fileSet = args[0];


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<ITenantContext, TenantContext>(s => new TenantContext(new Tenant(fileSet)));
builder.Services.AddTransient<ITenantAware, TenantAware>();

//builder.Services.AddTransient<IFileRepository>();
//builder.Services.AddTransient<IStorageLocationRepository>();
//builder.Services.AddTransient<IFileChecksumGenerator, >();
builder.Services.AddTransient<IFileIntegrityVerifier, LocalFileSystemFileIntegrityVerifier>();

builder.Services.AddTransient<IDirectoryVerificationService, DirectoryVerificationService>();
builder.Services.AddTransient<IIntegrityReporter, HtmlIntegrityReporter>();
builder.Services.AddTransient<IConsoleHandler, ConsoleHandler>();

using var host = builder.Build();


await host.RunAsync();