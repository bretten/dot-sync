using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Contracts;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;

public sealed record VerifyParameters(
    StorageLocation Source,
    string? Path,
    string? PathsToSkip) : IJobParameters
{
    public JobType Type => JobType.Verify;

    public IReadOnlyDictionary<string, string> AsKeyValuePairs()
    {
        return new Dictionary<string, string>()
        {
            { "Source", $"{Source.Type} - {Source.Path.Value}" },
            { "Path", Path ?? string.Empty },
            { "PathsToSkip", PathsToSkip! }
        }.AsReadOnly();
    }
}