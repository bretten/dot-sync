namespace com.brettnamba.DotSync.FileSystem.Application.Jobs;

/// <summary>
/// Job parameters
/// </summary>
public interface IJobParameters
{
    JobType Type { get; }

    IReadOnlyDictionary<string, string> AsKeyValuePairs();
}