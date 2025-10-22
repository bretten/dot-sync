using com.brettnamba.DotSync.Common.Domain.Tenants;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration.Exceptions;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Repositories;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration;

public sealed class FilePusher(
    IStorageLocationRepository storageLocationRepository,
    IFileRepository fileRepository,
    ITenantContext tenantContext,
    IFileCopier fileCopier,
    ILogger<FilePusher> logger) : TenantAware(tenantContext), IFilePusher
{
    public async Task<IEnumerable<DotFile>> PushUnverifiedFiles(StorageLocationType sourceType,
        FileSystemPath sourceRootPath, StorageLocationType destinationType, FileSystemPath destinationRootPath)
    {
        var source = await storageLocationRepository.GetByTypeAndPath(sourceType, sourceRootPath);
        var destination = await storageLocationRepository.GetByTypeAndPath(destinationType, destinationRootPath);
        if (source == null || destination == null)
        {
            throw new DirectoryNotStorageLocationException(
                $"No storage location for file set {TenantContext.CurrentTenant}");
        }

        if (source.Type != StorageLocationType.Local)
        {
            throw new NotSupportedException("Can only copy from local");
        }

        var unverifiedFiles = (await fileRepository.GetUnverifiedFiles()).ToList();

        var pushedFiles = new List<DotFile>();
        foreach (var file in unverifiedFiles)
        {
            //logger.LogInformation($"Uploading {file.Path}");
            var pushed = await fileCopier.CopyFile(sourceRootPath, file.Path, destination.Path);
            if (pushed) pushedFiles.Add(file);
        }

        return pushedFiles;
    }

    public async Task<IEnumerable<DotFile>> PushFilesInDir(StorageLocationType sourceType,
        FileSystemPath sourceRootPath, FileSystemPath sourcePushPath, StorageLocationType destinationType,
        FileSystemPath destinationRootPath)
    {
        var source = await storageLocationRepository.GetByTypeAndPath(sourceType, sourceRootPath);
        var destination = await storageLocationRepository.GetByTypeAndPath(destinationType, destinationRootPath);
        if (source == null || destination == null)
        {
            throw new DirectoryNotStorageLocationException(
                $"No storage location for file set {TenantContext.CurrentTenant}");
        }

        if (source.Type != StorageLocationType.Local)
        {
            throw new NotSupportedException("Can only copy from local");
        }

        var files = (await fileRepository.GetFilesByPath(sourcePushPath)).ToList();

        var pushedFiles = new List<DotFile>();
        foreach (var file in files)
        {
            //logger.LogInformation($"Uploading {file.Path}");
            var pushed = await fileCopier.CopyFile(sourceRootPath, file.Path, destination.Path);
            if (pushed) pushedFiles.Add(file);
        }


        return pushedFiles;
    }
}