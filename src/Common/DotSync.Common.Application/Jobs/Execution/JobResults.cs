namespace com.brettnamba.DotSync.Common.Application.Jobs.Execution;

/// <summary>
/// Result data collection for a job
/// </summary>
public sealed class JobResults
{
    public JobResults(IReadOnlyDictionary<string, ResultCollection> results)
    {
        Results = results;
    }

    /// <summary>
    /// Key-based results. Key is the name
    /// </summary>
    public IReadOnlyDictionary<string, ResultCollection> Results { get; }
}

/// <summary>
/// Result data. A collection of string arrays, where a single string array represents a *single* result item, but with
/// multiple points of data
/// </summary>
public sealed class ResultCollection
{
    /// <summary>
    /// The results
    /// </summary>
    public IReadOnlyList<string[]> Items { get; }

    /// <summary>
    /// True if this result information is only a quantity (<see cref="Count"/>)
    /// </summary>
    public bool IsOnlyCount { get; }

    /// <summary>
    /// The result count
    /// </summary>
    public long Count { get; }

    private ResultCollection(IReadOnlyList<string[]> items, bool isOnlyCount, long count)
    {
        Items = items;
        IsOnlyCount = isOnlyCount;
        Count = count;
    }

    public static ResultCollection Collection(IReadOnlyList<string[]> results)
    {
        return new ResultCollection(results, false, results.Count);
    }

    public static ResultCollection Quantity(long count)
    {
        return new ResultCollection([], true, count);
    }
}