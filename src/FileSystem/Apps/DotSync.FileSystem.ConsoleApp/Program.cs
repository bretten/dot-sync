using com.brettnamba.DotSync.FileSystem.Application.Console;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Application.Reporting;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

//builder.Services.AddTransient<IFileRepository>();
//builder.Services.AddTransient<IStorageLocationRepository>();
//builder.Services.AddTransient<IFileChecksumGenerator, >();
builder.Services.AddTransient<IFileIntegrityVerifier, LocalFileSystemFileIntegrityVerifier>();

builder.Services.AddTransient<IDirectoryVerificationService, DirectoryVerificationService>();
builder.Services.AddTransient<IIntegrityReporter, HtmlIntegrityReporter>();
builder.Services.AddTransient<IConsoleHandler, ConsoleHandler>();

using var host = builder.Build();


await host.RunAsync();