using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;

/// <summary>
/// Defines a storage location for files
/// </summary>
/// <param name="directoryPath">The path to the storage location</param>
/// <param name="fileCount">The total number of files in the storage location</param>
/// <param name="storageSize">The total size of the storage location</param>
public sealed class StorageLocation(FileSystemPath directoryPath, long fileCount, long storageSize)
{
    /// <summary>
    /// The path to the storage location
    /// </summary>
    public FileSystemPath DirectoryPath { get; } = directoryPath;

    /// <summary>
    /// The total number of files in the storage location
    /// </summary>
    public long FileCount { get; private set; } = fileCount;

    /// <summary>
    /// The total size of the storage location
    /// </summary>
    public long StorageSize { get; private set; } = storageSize;

    /// <summary>
    /// Updates the file count
    /// </summary>
    public void UpdateFileCount(long fileCount)
    {
        FileCount = fileCount;
    }

    /// <summary>
    /// Updates the storage size
    /// </summary>
    public void UpdateStorageSize(long storageSize)
    {
        StorageSize = storageSize;
    }
}