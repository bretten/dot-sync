using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;

public interface IStorageLocationRepository
{
    Task Add(StorageLocation storageLocation);
    Task Update(StorageLocation storageLocation);
    Task<StorageLocation?> GetByTypeAndPath(StorageLocationType type, FileSystemPath path);
    Task<IEnumerable<StorageLocation>> GetAll();
}