using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;

public interface IFileRepository
{
    Task Add(DotFile file);
    Task AddSyncedFile(Guid fileId, Guid storageLocationId);
    Task Update(DotFile file);
    Task<DotFile?> GetFileByChecksum(FileSha256Checksum checksum);
    Task<DotFile?> GetFileByPath(FileSystemPath path);
    Task SetAllAsUnverified(FileSystemPath path);
    Task<IEnumerable<DotFile>> GetUnverifiedFiles();
    Task<IEnumerable<DotFile>> GetFilesByPath(FileSystemPath path);
}