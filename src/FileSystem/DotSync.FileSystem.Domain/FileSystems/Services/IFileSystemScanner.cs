using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;

/// <summary>
/// Defines a service that scans a directory for new files that have not been synced with the domain
/// </summary>
public interface IFileSystemScanner
{
    /// <summary>
    /// Should scan the specified path for new files that have not yet been synced with the domain
    /// </summary>
    /// <param name="storageLocation">The storage to scan for new files</param>
    /// <param name="pathPrefix">Scan for files with this path prefix</param>
    /// <returns><see cref="FileSystemScannerResult"/></returns>
    Task<FileSystemScannerResult> Scan(StorageLocation storageLocation, string pathPrefix);
}