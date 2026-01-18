using com.brettnamba.DotSync.Common.Application.Jobs;
using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;

public sealed record PushByStorageParameters(
    StorageLocation Source,
    string PathPrefixFilter,
    long UploadLimitMb,
    StorageLocation Destination) : IJobParameters
{
    public JobType Type => JobType.PushByStorage;

    public IReadOnlyDictionary<string, string> AsKeyValuePairs()
    {
        return new Dictionary<string, string>()
        {
            { "Source", $"{Source.Type} - {Source.Path.Value}" },
            { "PathPrefixFilter", PathPrefixFilter },
            { "UploadLimitMb", UploadLimitMb.ToString() },
            { "Destination", $"{Destination.Type} - {Destination.Path.Value}" }
        }.AsReadOnly();
    }
}