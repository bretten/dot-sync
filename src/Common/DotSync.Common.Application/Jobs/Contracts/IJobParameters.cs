namespace com.brettnamba.DotSync.Common.Application.Jobs.Contracts;

/// <summary>
/// Job parameters
/// </summary>
public interface IJobParameters
{
    JobType Type { get; }

    IReadOnlyDictionary<string, string> AsKeyValuePairs();
}