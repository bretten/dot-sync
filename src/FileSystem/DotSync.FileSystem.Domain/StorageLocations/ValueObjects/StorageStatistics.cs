namespace com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.ValueObjects;

/// <summary>
/// Statistics about a storage
/// </summary>
public readonly record struct StorageStatistics
{
    /// <summary>
    /// The total number of files
    /// </summary>
    public long FileCount { get; }

    /// <summary>
    /// The total size of the storage
    /// </summary>
    public long Size { get; }

    public StorageStatistics(long fileCount, long size)
    {
        if (fileCount < 0 || size < 0)
        {
            throw new InvalidStorageStatisticAmountException(
                $"Invalid storage stats. Must not be less than 0. File count: {fileCount}, Total size: {size}");
        }

        FileCount = fileCount;
        Size = size;
    }

    private sealed class InvalidStorageStatisticAmountException(string? message) : Exception(message);
}