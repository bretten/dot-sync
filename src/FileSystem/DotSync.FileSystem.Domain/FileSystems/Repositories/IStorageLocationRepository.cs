using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;

public interface IStorageLocationRepository
{
    Task Add(StorageLocation storageLocation);
    Task<bool> Exists(StorageLocationType type, StoragePath path);

    /// <summary>
    /// Returns the main storage location
    /// </summary>
    /// <returns>Main storage location</returns>
    Task<StorageLocation> GetMainStorageLocation();

    Task<IEnumerable<StorageLocation>> GetAll();
    Task<IEnumerable<StorageLocation>> GetAllButMainStorage();

    /// <summary>
    /// Returns the full local file path for the file in the main storage
    /// </summary>
    /// <param name="filePath">Relative file path</param>
    /// <returns>Full local file path</returns>
    Task<string> GetPathInMainStorage(FileSystemPath filePath);
}