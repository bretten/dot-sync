using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;

public sealed record PushParameters(
    string? SourceRootPath,
    string? SourcePushPath,
    StorageLocationType DestinationType,
    string? DestinationPath) : IJobParameters
{
    public string JobId => "Push";

    public IReadOnlyDictionary<string, string> AsKeyValuePairs()
    {
        return new Dictionary<string, string>()
        {
            { "SourceRootPath", SourceRootPath! },
            { "SourcePushPath", SourcePushPath! },
            { "StorageLocationType", DestinationType.ToString() },
            { "DestinationPath", DestinationPath! },
        }.AsReadOnly();
    }
}