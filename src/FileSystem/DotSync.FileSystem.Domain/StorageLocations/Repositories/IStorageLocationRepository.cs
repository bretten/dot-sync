using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;

namespace com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Repositories;

public interface IStorageLocationRepository
{
    Task Add(StorageLocation storageLocation);
    Task Update(StorageLocation storageLocation);
    Task<StorageLocation?> GetByTypeAndPath(StorageLocationType type, FileSystemPath path);
}