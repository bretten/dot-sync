using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;

/// <summary>
/// Defines a storage location for files
/// </summary>
public sealed class StorageLocation
{
    /// <summary>
    /// ID
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// The type of storage location
    /// </summary>
    public StorageLocationType Type { get; }

    /// <summary>
    /// The path to the storage location
    /// </summary>
    public FileSystemPath Path { get; }

    /// <summary>
    /// The total number of files and size of the storage location
    /// </summary>
    public StorageStatistics StorageStatistics { get; private set; }

    /// <summary>
    /// Previous storage statistics
    /// </summary>
    public List<HistoricalStorageStatistics> HistoricalStorageStatistics { get; } = [];

    /// <summary>
    /// Constructor
    /// </summary>
    public StorageLocation(StorageLocationType type, FileSystemPath path, StorageStatistics storageStatistics)
    {
        Id = Guid.NewGuid();
        Type = type;
        Path = path;
        StorageStatistics = storageStatistics;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public StorageLocation(StorageLocationType type, FileSystemPath path)
    {
        Type = type;
        Path = path;
    }

    /// <summary>
    /// Updates the storage statistics and logs the last statistics
    /// </summary>
    public void UpdateStatistics(long fileCount, long storageSize, DateTimeOffset dateTime)
    {
        HistoricalStorageStatistics.Add(new HistoricalStorageStatistics(dateTime, StorageStatistics));

        StorageStatistics = new StorageStatistics(fileCount: fileCount, size: storageSize);
    }
}