using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;

namespace com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Repositories;

public interface IStorageLocationRepository
{
    Task Add(StorageLocation storageLocation);
    Task Update(StorageLocation storageLocation);
    Task<StorageLocation?> GetByType(StorageLocationType type);
}