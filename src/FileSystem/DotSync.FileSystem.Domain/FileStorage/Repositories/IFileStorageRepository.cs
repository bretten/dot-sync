using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Repositories;

public interface IFileStorageRepository
{
    Task Add(DotFile file);
    Task Update(DotFile file);
    Task<DotFile?> GetFileByChecksum(FileSha256Checksum checksum);
    Task<DotFile?> GetFileByPath(FileSystemPath path);
    Task SetAllAsUnverified(StorageLocation storageLocation);
    Task<IEnumerable<DotFile>> GetUnverifiedFiles();

    Task Add(StorageLocation storageLocation);
    Task Update(StorageLocation storageLocation);
    Task<StorageLocation?> GetByTypeAndPath(StorageLocationType type, FileSystemPath path);
}