using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Entities;

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
    public FileSystemPath Path { get; private set; }

    /// <summary>
    /// The total number of files and size of the storage location
    /// </summary>
    public StorageStatistics StorageStatistics { get; private set; }

    /// <summary>
    /// Previous storage statistics
    /// </summary>
    public List<HistoricalStorageStatistics> HistoricalStorageStatistics { get; } = [];
    
    public List<DotFile> Files { get; } = [];

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
        StorageStatistics = new StorageStatistics(fileCount: fileCount, size: storageSize);

        HistoricalStorageStatistics.Add(new HistoricalStorageStatistics(dateTime, StorageStatistics));
    }

    public void UpdatePath(FileSystemPath path)
    {
        Path = path;
    }
}