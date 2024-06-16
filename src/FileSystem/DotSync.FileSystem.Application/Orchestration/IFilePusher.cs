using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;

namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration;

public interface IFilePusher
{
    Task<IEnumerable<DotFile>> PushFiles(StorageLocationType sourceType, FileSystemPath sourcePath,
        StorageLocationType destinationType, FileSystemPath destinationPath);
}