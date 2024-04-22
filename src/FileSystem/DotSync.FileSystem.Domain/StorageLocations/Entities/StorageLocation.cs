using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;

/// <summary>
/// Defines a storage location for files
/// </summary>
public sealed class StorageLocation(FileSystemPath path, StorageLocationType type, StorageStatistics storageStatistics)
{
    /// <summary>
    /// The path to the storage location
    /// </summary>
    public FileSystemPath Path { get; } = path;

    /// <summary>
    /// The type of storage location
    /// </summary>
    public StorageLocationType Type { get; } = type;

    /// <summary>
    /// The total number of files and size of the storage location
    /// </summary>
    public StorageStatistics StorageStatistics { get; private set; } = storageStatistics;

    /// <summary>
    /// Previous storage statistics
    /// </summary>
    public List<HistoricalStorageStatistics> HistoricalStorageStatistics { get; } = [];

    /// <summary>
    /// Updates the storage statistics and logs the last statistics
    /// </summary>
    public void UpdateStatistics(long fileCount, long storageSize, DateTime dateTime)
    {
        HistoricalStorageStatistics.Add(new HistoricalStorageStatistics(dateTime, StorageStatistics));

        StorageStatistics = new StorageStatistics(fileCount: fileCount, size: storageSize);
    }
}