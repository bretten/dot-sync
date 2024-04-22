namespace com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.ValueObjects;

/// <summary>
/// Represents a previous storage statistic
/// </summary>
/// <param name="DateTime">The time of the previous statistics</param>
/// <param name="Statistics">The storage statistics</param>
public readonly record struct HistoricalStorageStatistics(DateTime DateTime, StorageStatistics Statistics);