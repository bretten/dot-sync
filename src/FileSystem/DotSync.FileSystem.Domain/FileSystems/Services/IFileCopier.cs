using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;

/// <summary>
/// Defines a service that copies a file
/// </summary>
public interface IFileCopier
{
    Task<bool> CopyFile(StorageLocation source, DotFile file, StorageLocation destination);
}