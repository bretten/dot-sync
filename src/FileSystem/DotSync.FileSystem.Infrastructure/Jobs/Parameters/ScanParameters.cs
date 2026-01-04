using com.brettnamba.DotSync.FileSystem.Application.Jobs;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;

public sealed record ScanParameters(
    string? Path) : IJobParameters
{
    public string JobId => "Scan";

    public IReadOnlyDictionary<string, string> AsKeyValuePairs()
    {
        return new Dictionary<string, string>()
        {
            { "Path", Path! }
        }.AsReadOnly();
    }
}