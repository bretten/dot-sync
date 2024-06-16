using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;

namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration;

public interface IFilePusher
{
    Task<IEnumerable<DotFile>> PushUnverifiedFiles(StorageLocationType sourceType, FileSystemPath sourceRootPath,
        StorageLocationType destinationType, FileSystemPath destinationRootPath);

    Task<IEnumerable<DotFile>> PushFilesInDir(StorageLocationType sourceType, FileSystemPath sourceRootPath,
        FileSystemPath sourcePushPath, StorageLocationType destinationType, FileSystemPath destinationPath);
}