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
    Task SetAllAsUnverified(string pathPrefix);
    Task<IEnumerable<DotFile>> GetUnverifiedFiles();
    Task<IEnumerable<DotFile>> GetFilesByPathPrefix(string pathPrefix);

    /// <summary>
    /// Returns files that do not have a row in the synced files join table corresponding to all storage locations
    /// </summary>
    /// <returns><see cref="DotFile"/>s that are not fully synced with all storage locations</returns>
    Task<IEnumerable<DotFile>> GetUnsyncedFiles();

    /// <summary>
    /// Returns files that have no corresponding row in the synced files join table
    /// </summary>
    /// <param name="storageLocationId">The storage location to check</param>
    /// <returns><see cref="DotFile"/>s that have not been synced with the storage location</returns>
    Task<IEnumerable<DotFile>> GetUnsyncedFiles(Guid storageLocationId);

    /// <summary>
    /// Returns files that have no corresponding row in the synced files join table and filters on file path prefix
    /// </summary>
    /// <param name="storageLocationId">The storage location to check</param>
    /// <param name="pathPrefix">File path prefix filter</param>
    /// <returns><see cref="DotFile"/>s that have not been synced with the storage location</returns>
    Task<IEnumerable<DotFile>> GetUnsyncedFiles(Guid storageLocationId, string pathPrefix);
}