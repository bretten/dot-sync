namespace com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.ValueObjects;

/// <summary>
/// Represents a previous storage statistic
/// </summary>
public sealed record HistoricalStorageStatistics
{
    /// <summary>
    /// The time of the previous statistics
    /// </summary>
    public DateTimeOffset DateTime { get; }

    /// <summary>
    /// The storage statistics
    /// </summary>
    public StorageStatistics Statistics { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    public HistoricalStorageStatistics(DateTimeOffset dateTime, StorageStatistics statistics)
    {
        DateTime = dateTime;
        Statistics = statistics;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public HistoricalStorageStatistics(DateTimeOffset dateTime)
    {
        DateTime = dateTime;
    }
};