namespace com.brettnamba.DotSync.FileSystem.Application.Jobs;

/// <summary>
/// Job parameters
/// </summary>
public interface IJobParameters
{
    string JobId { get; }

    IReadOnlyDictionary<string, string> AsKeyValuePairs();
}