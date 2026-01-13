using System.ComponentModel;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Domain.Tenants;
using com.brettnamba.DotSync.Common.Extensions;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Application.Reporting;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileOrganization.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
        "scan" => Scan(remainingArgs.ToArray()),
        "push" => Push(remainingArgs.ToArray()),
        "push_dir" => PushDir(remainingArgs.ToArray()),
        "storage_location" => StorageLocationAction(remainingArgs.ToArray()),
        "sort" => Sort(remainingArgs.ToArray()),
        _ => throw new UnknownCommandException($"Unknown command {command}")
    };
}

static async Task Verify(string[] args)
{
    if (args.Length != 6)
    {
        throw new RequiredArgumentNotProvided(
            "Could not run verify. Parameter order is file set, storage location type, path in storage location, verify path, paths to skip CSV, report output path");
    }

    var fileSet = args[0];
    var storageLocationType =
        (StorageLocationType)TypeDescriptor.GetConverter(typeof(StorageLocationType)).ConvertFrom(args[1])!;
    var storageLocationPath = FileSystemPath.Create(args[2], replaceBackslashes: OperatingSystem.IsWindows());
    var verifyPath =
        FileSystemPath.Create(args[3] == "." ? "" : args[3], replaceBackslashes: OperatingSystem.IsWindows());
    var pathsToSkip = args[4] == "."
        ? new List<FileSystemPath>()
        : args[4].Split(",").Select(FileSystemPath.Create);
    var reportOutputPath = args[5];

    var builder = ConfigureAndRegisterServices(fileSet, storageLocationType);
    using var host = builder.Build();

    var scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
    using var scope = scopeFactory.CreateScope();
    var service = scope.ServiceProvider.GetService<IStorageLocationIntegrityVerificationService>();
    var clock = host.Services.GetRequiredService<IClock>();
    if (service == null)
    {
        throw new ServiceNotFoundException(
            $"Verify could not resolve service of type {nameof(IStorageLocationIntegrityVerificationService)}");
    }

    var result = await service.Execute(storageLocationType, storageLocationPath, verifyPath, pathsToSkip);
    Console.WriteLine($"Storage location type: {result.StorageLocation.Type.GetDisplayName()}");
    Console.WriteLine($"Storage location path: {result.StorageLocation.Path.Value}");
    var reportFileName =
        $"verify_{fileSet}_{storageLocationType.GetDisplayName()}_{clock.GetUtcNow().ToString("yyyyMMddTHHmmss")}.html";
    await File.WriteAllTextAsync($"{reportOutputPath}{Path.AltDirectorySeparatorChar}{reportFileName}", result.Report);

    await host.StopAsync();
}

static async Task Scan(string[] args)
{
    if (args.Length != 4)
    {
        throw new RequiredArgumentNotProvided(
            "Could not run scan. Parameter order is file set, storage location type, path in storage location, report output path");
    }

    var fileSet = args[0];
    var storageLocationType =
        (StorageLocationType)TypeDescriptor.GetConverter(typeof(StorageLocationType)).ConvertFrom(args[1])!;
    var storageLocationPath = FileSystemPath.Create(args[2], replaceBackslashes: OperatingSystem.IsWindows());
    var reportOutputPath = args[3];

    var builder = ConfigureAndRegisterServices(fileSet, storageLocationType);
    using var host = builder.Build();

    var service = host.Services.GetService<IFileSystemScanner>();
    var clock = host.Services.GetRequiredService<IClock>();
    if (service == null)
    {
        throw new ServiceNotFoundException($"Scan could not resolve service of type {nameof(IFileSystemScanner)}");
    }

    //var result = await service.Scan(storageLocationPath);
    // Console.WriteLine("New Files:");
    // // foreach (var newFile in result.NewFiles)
    // // {
    // //     Console.WriteLine($"{newFile.Item2.Value}\t{newFile.Item1.Value}");
    // // }
    //
    // var reportFileName =
    //     $"scan_{fileSet}_{storageLocationType.GetDisplayName()}_{clock.GetUtcNow().ToString("yyyyMMddTHHmmss")}.html";
    // await File.WriteAllTextAsync($"{reportOutputPath}{Path.AltDirectorySeparatorChar}{reportFileName}",
    //     string.Join("<br/>",
    //         result.NewFiles.Select(x => $"{x.Item2.Value}&nbsp;&nbsp;&nbsp;&nbsp;{x.Item1.Value}")));

    await host.StopAsync();
}

static async Task Push(string[] args)
{
    if (args.Length != 5)
    {
        throw new RequiredArgumentNotProvided(
            "Could not run push. Parameter order is file set, source path, destination storage location type, destination path in storage location, report output path");
    }

    var fileSet = args[0];
    var sourcePath = FileSystemPath.Create(args[1], replaceBackslashes: OperatingSystem.IsWindows());
    var destinationType =
        (StorageLocationType)TypeDescriptor.GetConverter(typeof(StorageLocationType)).ConvertFrom(args[2])!;
    var destinationPath = FileSystemPath.Create(args[3], replaceBackslashes: OperatingSystem.IsWindows());
    var reportOutputPath = args[4];

    var builder = ConfigureAndRegisterServices(fileSet, destinationType);
    using var host = builder.Build();

    var service = host.Services.GetService<IFilePusher>();
    var clock = host.Services.GetRequiredService<IClock>();
    if (service == null)
    {
        throw new ServiceNotFoundException($"Scan could not resolve service of type {nameof(IFileSystemScanner)}");
    }

    // var result =
    //     await service.PushUnverifiedFiles(StorageLocationType.Local, sourcePath, destinationType, destinationPath);
    // Console.WriteLine("New Files:");
    // foreach (var newFile in result)
    // {
    //     Console.WriteLine($"{newFile.Sha256Checksum.Value}\t{newFile.Path.Value}");
    // }
    //
    // var reportFileName =
    //     $"push_{fileSet}_{destinationType.GetDisplayName()}_{clock.GetUtcNow().ToString("yyyyMMddTHHmmss")}.html";
    // await File.WriteAllTextAsync($"{reportOutputPath}{Path.AltDirectorySeparatorChar}{reportFileName}",
    //     string.Join("<br/>",
    //         result.Select(x => $"{x.Sha256Checksum.Value}&nbsp;&nbsp;&nbsp;&nbsp;{x.Path.Value}")));

    await host.StopAsync();
}

static async Task PushDir(string[] args)
{
    if (args.Length != 6)
    {
        throw new RequiredArgumentNotProvided(
            "Could not run push. Parameter order is file set, source root path, source push path, destination storage location type, destination path in storage location, report output path");
    }

    var fileSet = args[0];
    var sourceRootPath = FileSystemPath.Create(args[1], replaceBackslashes: OperatingSystem.IsWindows());
    var sourcePushPath = FileSystemPath.Create(args[2], replaceBackslashes: OperatingSystem.IsWindows());
    var destinationType =
        (StorageLocationType)TypeDescriptor.GetConverter(typeof(StorageLocationType)).ConvertFrom(args[3])!;
    var destinationRootPath = FileSystemPath.Create(args[4], replaceBackslashes: OperatingSystem.IsWindows());
    var reportOutputPath = args[5];

    var builder = ConfigureAndRegisterServices(fileSet, destinationType);
    using var host = builder.Build();

    var scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
    using var scope = scopeFactory.CreateScope();
    var service = scope.ServiceProvider.GetService<IFilePusher>();
    var clock = scope.ServiceProvider.GetRequiredService<IClock>();
    if (service == null)
    {
        throw new ServiceNotFoundException($"Scan could not resolve service of type {nameof(IFileSystemScanner)}");
    }

    // var result = await service.PushFilesByPath(StorageLocationType.Local, sourceRootPath, sourcePushPath,
    //     destinationType, destinationRootPath);
    // Console.WriteLine("New Files:");
    // foreach (var newFile in result)
    // {
    //     Console.WriteLine($"{newFile.Sha256Checksum.Value}\t{newFile.Path.Value}");
    // }

    // var reportFileName =
    //     $"push_{fileSet}_{destinationType.GetDisplayName()}_{clock.GetUtcNow().ToString("yyyyMMddTHHmmss")}.html";
    // await File.WriteAllTextAsync($"{reportOutputPath}{Path.AltDirectorySeparatorChar}{reportFileName}",
    //     string.Join("<br/>",
    //         result.Select(x => $"{x.Sha256Checksum.Value}&nbsp;&nbsp;&nbsp;&nbsp;{x.Path.Value}")));

    await host.StopAsync();
}

static async Task StorageLocationAction(string[] args)
{
    if (args.Length < 4 || args.Length > 5)
    {
        throw new RequiredArgumentNotProvided(
            "Could not run storage_location. Parameter order is storage location action, file set, storage location type, path to storage location, new path");
    }

    var action = args[0];
    var fileSet = args[1];
    var storageLocationType =
        (StorageLocationType)TypeDescriptor.GetConverter(typeof(StorageLocationType)).ConvertFrom(args[2])!;
    var storageLocationPath = FileSystemPath.Create(args[3], replaceBackslashes: OperatingSystem.IsWindows());

    var builder = ConfigureAndRegisterServices(fileSet, storageLocationType);
    using var host = builder.Build();

    var service = host.Services.GetService<IStorageLocationRepository>();
    if (service == null)
    {
        throw new ServiceNotFoundException(
            $"Verify could not resolve service of type {nameof(IStorageLocationRepository)}");
    }

    switch (action)
    {
        case "create":
            await service.Add(new StorageLocation(storageLocationType, storageLocationPath));
            break;
        case "update":
            var storageLocation = await service.GetByTypeAndPath(storageLocationType, storageLocationPath);
            if (storageLocation == null) throw new StorageLocationNotFoundException();
            if (args.Length != 5) throw new ArgumentException("No new path specified for storage location");
            storageLocation.UpdatePath(FileSystemPath.Create(args[4], replaceBackslashes: OperatingSystem.IsWindows()));
            await service.Update(storageLocation);
            break;
        default:
            throw new InvalidStorageLocationActionException($"Invalid storage location action: {action}");
    }
}

static async Task Sort(string[] args)
{
    if (args.Length != 2)
    {
        throw new RequiredArgumentNotProvided("Could not run sort. Parameter order is source path, destination path");
    }

    var sourcePath = FileSystemPath.Create(args[0]);
    var destinationPath = FileSystemPath.Create(args[1]);

    var builder = Host.CreateApplicationBuilder();
    var env = builder.Environment.EnvironmentName;
    builder.Configuration.AddJsonFile("appsettings.json");
    builder.Configuration.AddJsonFile($"appsettings.{env}.json", optional: true);
    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddUserSecrets<Program>();
    }

    builder.Services.AddLogging();
    builder.Services.AddTransient<IFileMetadataReader, WindowsFileMetadataReader>();
    builder.Services.AddTransient<IFileSorter, LocalFileSystemByDateFileSorter>();

    using var host = builder.Build();

    var service = host.Services.GetRequiredService<IFileSorter>();
    if (service == null)
    {
        throw new ServiceNotFoundException($"Verify could not resolve service of type {nameof(IFileSorter)}");
    }

    await service.Sort(sourcePath, destinationPath);
}

static HostApplicationBuilder ConfigureAndRegisterServices(string fileSet, StorageLocationType storageLocationType)
{
    var builder = Host.CreateApplicationBuilder();

    var env = builder.Environment.EnvironmentName;
    builder.Configuration.AddJsonFile("appsettings.json");
    builder.Configuration.AddJsonFile($"appsettings.{env}.json", optional: true);
    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddUserSecrets<Program>();
    }

    builder.Services.AddTransient<ITenantContext, TenantContext>(s => new TenantContext(new Tenant(fileSet)));
    builder.Services.AddTransient<ITenantAware, TenantAware>();
    builder.Services.AddTransient<IClock, Clock>();

    builder.Services.AddDbContextFactory<FileSystemsDbContext>(optionsBuilder =>
    {
        optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString($"FileSystems_{fileSet}"));
    });
    builder.Services.AddDbContextFactory<FileSystemsDbContext>(optionsBuilder =>
            optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString($"FileSystems_{fileSet}")),
        ServiceLifetime.Scoped
    );
    builder.Services.AddTransient<IFileRepository, EntityFrameworkCoreFileRepository>();
    // builder.Services.AddTransient<IFileRepository, NpgsqlFileRepository>(sp =>
    // {
    //     var dataSource =
    //         new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString($"FileSystems_{fileSet}"))
    //             .Build();
    //     return new NpgsqlFileRepository(dataSource, sp.GetRequiredService<IClock>());
    // });
    builder.Services.AddTransient<IStorageLocationRepository, EntityFrameworkCoreStorageLocationRepository>();
    builder.Services.AddTransient<IFileChecksumGenerator, Sha256FileChecksumGenerator>();
    builder.Services.AddTransient<IFileMetadataReader, WindowsFileMetadataReader>();
    builder.Services.AddTransient<IFileIntegrityVerifierFactory, FileIntegrityVerifierFactory>();
    builder.Services.AddTransient<IAmazonS3>(sp =>
    {
        var awsAccessKeyId = builder.Configuration[$"AmazonS3:{fileSet}:AwsAccessKey"];
        var awsSecretAccessKey = builder.Configuration[$"AmazonS3:{fileSet}:AwsSecretAccessKey"];
        var region = builder.Configuration[$"AmazonS3:{fileSet}:Region"];
        return new AmazonS3Client(new BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey),
            RegionEndpoint.GetBySystemName(region));
    });
    builder.Services.AddTransient<IFileSystemScanner, LocalFileSystemScanner>();
    builder.Services.AddTransient<IFileCopier, AmazonS3FileCopier>(sp =>
    {
        var storageClass = S3StorageClass.FindValue(builder.Configuration[$"AmazonS3:{fileSet}:StorageClass"]) ??
                           throw new ArgumentException($"Storage class not defined for {fileSet}");

        return new AmazonS3FileCopier(sp.GetRequiredService<IFileChecksumGenerator>(),
            sp.GetRequiredService<IAmazonS3>(), storageClass, sp.GetRequiredService<ILogger<AmazonS3FileCopier>>());
    });

    builder.Services
        .AddTransient<IStorageLocationIntegrityVerificationService, StorageLocationIntegrityVerificationService>();
    builder.Services.AddTransient<IIntegrityReporter, HtmlIntegrityReporter>();
    builder.Services.AddTransient<IFilePusher, FilePusher>();

    return builder;
}

internal sealed class UnknownCommandException(string? message) : Exception(message);

internal sealed class RequiredArgumentNotProvided(string? message) : Exception(message);

internal sealed class ServiceNotFoundException(string? message) : Exception(message);

internal sealed class StorageLocationNotFoundException(string? message = "No storage location found")
    : Exception(message);

internal sealed class InvalidStorageLocationActionException(string? message) : Exception(message);