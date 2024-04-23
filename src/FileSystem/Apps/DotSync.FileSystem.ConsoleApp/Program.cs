using System.ComponentModel;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Domain.Tenants;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Application.Reporting;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Repositories;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using com.brettnamba.DotSync.FileSystem.Infrastructure.StorageLocations.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var commandTask = DetermineCommand(args);

await commandTask;

return;


static Task DetermineCommand(string[] args)
{
    if (args.Length < 1)
    {
        throw new RequiredArgumentNotProvided("No arguments provided");
    }

    // Determine the type of command
    var command = args.First();
    var remainingArgs = new ArraySegment<string>(args)[1..];

    return command.ToLower() switch
    {
        "verify" => Verify(remainingArgs.ToArray()),
        _ => throw new UnknownCommandException($"Unknown command {command}")
    };
}

static async Task Verify(string[] args)
{
    if (args.Length != 4)
    {
        throw new RequiredArgumentNotProvided(
            "Could not run verify. Parameter order is file set, storage location type, path in storage location, report output path");
    }

    var fileSet = args[0];
    var storageLocationType =
        (StorageLocationType)TypeDescriptor.GetConverter(typeof(StorageLocationType)).ConvertFrom(args[1])!;
    var storageLocationPath = FileSystemPath.Create(args[2], replaceBackslashes: OperatingSystem.IsWindows());
    var reportOutputPath = args[3];

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddTransient<ITenantContext, TenantContext>(s => new TenantContext(new Tenant(fileSet)));
    builder.Services.AddTransient<ITenantAware, TenantAware>();
    builder.Services.AddTransient<IClock, Clock>();

    builder.Services.AddDbContext<FileSystemsDbContext>();
    builder.Services.AddTransient<IFileRepository, EntityFrameworkCoreFileRepository>();
    builder.Services.AddDbContext<StorageLocationsDbContext>();
    builder.Services.AddTransient<IStorageLocationRepository, EntityFrameworkCoreStorageLocationRepository>();
    builder.Services.AddTransient<IFileChecksumGenerator, Sha256FileChecksumGenerator>();
    builder.Services.AddTransient<IFileIntegrityVerifier, LocalFileSystemFileIntegrityVerifier>();

    builder.Services
        .AddTransient<IStorageLocationIntegrityVerificationService, StorageLocationIntegrityVerificationService>();
    builder.Services.AddTransient<IIntegrityReporter, HtmlIntegrityReporter>();


    using var host = builder.Build();

    var service = host.Services.GetService<IStorageLocationIntegrityVerificationService>();
    if (service == null)
    {
        throw new ServiceNotFoundException(
            $"Verify could not resolve service of type {nameof(IStorageLocationIntegrityVerificationService)}");
    }

    var result = await service.Execute(storageLocationType, storageLocationPath);
    Console.WriteLine($"Total files: {result.Result.FileCount}");
    Console.WriteLine($"Verified files: {result.Result.SuccessfulVerifications}");
    Console.WriteLine($"Unverified files: {result.Result.UnverifiedFiles.Count}");
    Console.WriteLine($"Files no longer in set: {result.Result.FilesNoLongerInSet.Count}");
    Console.WriteLine($"Total size (bytes): {result.Result.TotalSize}");
    await File.WriteAllTextAsync(reportOutputPath, result.Report);

    await host.StopAsync();
}

internal sealed class UnknownCommandException(string? message) : Exception(message);

internal sealed class RequiredArgumentNotProvided(string? message) : Exception(message);

internal sealed class ServiceNotFoundException(string? message) : Exception(message);