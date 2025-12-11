using com.brettnamba.DotSync.Common.Domain.Tenants;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration.Exceptions;
using com.brettnamba.DotSync.FileSystem.Application.Storage;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration;

public sealed class FilePusher(
    IStorageLocationRepository storageLocationRepository,
    IFileRepository fileRepository,
    ITenantContext tenantContext,
    IFileCopier fileCopier,
    IMainStorageProvider mainStorageProvider,
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
            var pushed = await fileCopier.CopyFile(sourceRootPath, file.Path, destination.Path);
            if (!pushed) continue;
            pushedFiles.Add(file);
            await fileRepository.AddSyncedFile(file.Id, destination.Id);
        }

        return pushedFiles;
    }

    public async Task<IEnumerable<DotFile>> PushFilesInStorage(Guid storageLocationId, string prefixFilter,
        int uploadLimitMb)
    {
        var storageLocations = await storageLocationRepository.GetAll();
        var destination = storageLocations.FirstOrDefault(x => x.Id == storageLocationId);
        if (destination == null)
        {
            throw new DirectoryNotStorageLocationException($"No storage location for {storageLocationId:D}");
        }

        var files = (await fileRepository.GetUnsyncedFiles(destination.Id, prefixFilter)).ToList();

        var mainStoragePath = await mainStorageProvider.GetMainStoragePath();
        var pushedFiles = new List<DotFile>();
        var uploadedBytes = 0L;
        var uploadLimitBytes = uploadLimitMb * 1000000; // Use MB, not MiB since that is the user-facing value
        foreach (var file in files)
        {
            // Loose limit for now, just iterate until the rough limit is reached
            var projectedTotalUploadAmount = uploadedBytes + file.Size;
            if (uploadLimitMb != 0 && projectedTotalUploadAmount > uploadLimitBytes)
            {
                continue;
            }

            var pushed = await fileCopier.CopyFile(mainStoragePath, file.Path, destination.Path);
            if (!pushed) continue;
            pushedFiles.Add(file);
            await fileRepository.AddSyncedFile(file.Id, destination.Id);
            uploadedBytes += file.Size;
        }

        return pushedFiles;
    }
}