using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;

/// <summary>
/// Defines a service that copies a file
/// </summary>
public interface IFileCopier
{
    /// <summary>
    /// Checks if the file exists at the destination
    /// </summary>
    /// <param name="file">The file to check</param>
    /// <param name="destination">Destination storage</param>
    /// <returns>True if the file exists, otherwise false</returns>
    Task<bool> Exists(DotFile file, StorageLocation destination);

    /// <summary>
    /// Copies the file to the destination
    /// </summary>
    /// <param name="source">The source storage location</param>
    /// <param name="file">The file to copy</param>
    /// <param name="destination">The destination storage</param>
    /// <returns></returns>
    Task<bool> CopyFile(StorageLocation source, DotFile file, StorageLocation destination);
}