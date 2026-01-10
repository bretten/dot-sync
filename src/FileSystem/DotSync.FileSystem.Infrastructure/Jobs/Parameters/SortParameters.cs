using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Contracts;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;

public sealed record SortParameters(string? SourcePath, string? DestinationPath)
    : IJobParameters
{
    public JobType Type => JobType.Sort;

    public IReadOnlyDictionary<string, string> AsKeyValuePairs()
    {
        return new Dictionary<string, string>()
        {
            { "SourcePath", SourcePath! },
            { "DestinationPath", DestinationPath! }
        }.AsReadOnly();
    }
}