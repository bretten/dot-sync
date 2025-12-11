using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration;

public interface IFilePusher
{
    Task<IEnumerable<DotFile>> PushUnverifiedFiles(StorageLocationType sourceType, FileSystemPath sourceRootPath,
        StorageLocationType destinationType, FileSystemPath destinationRootPath);

    Task<IEnumerable<DotFile>> PushFilesInDir(StorageLocationType sourceType, FileSystemPath sourceRootPath,
        FileSystemPath sourcePushPath, StorageLocationType destinationType, FileSystemPath destinationPath);

    Task<IEnumerable<DotFile>> PushFilesInStorage(Guid storageLocationId, string prefixFilter, int uploadLimitMb);
}