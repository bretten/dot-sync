namespace com.brettnamba.DotSync.Common.Application.Jobs.Execution;

/// <summary>
/// Result data for a job
/// </summary>
public sealed class JobResults
{
    public JobResults(IReadOnlyDictionary<string, IEnumerable<string[]>> resultLists)
    {
        ResultLists = resultLists;
    }

    /// <summary>
    /// Key-based results
    /// </summary>
    public IReadOnlyDictionary<string, IEnumerable<string[]>> ResultLists { get; }
}