using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Contracts;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;

public sealed record VerifyParameters(
    StorageLocationType StorageType,
    string? StoragePath,
    string? VerifyPath,
    string? PathsToSkip) : IJobParameters
{
    public JobType Type => JobType.Verify;

    public IReadOnlyDictionary<string, string> AsKeyValuePairs()
    {
        return new Dictionary<string, string>()
        {
            { "StorageType", StorageType.ToString() },
            { "StoragePath", StoragePath! },
            { "VerifyPath", VerifyPath! },
            { "PathsToSkip", PathsToSkip! }
        }.AsReadOnly();
    }
}