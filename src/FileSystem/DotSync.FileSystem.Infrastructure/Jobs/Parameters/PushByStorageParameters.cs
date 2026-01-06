using com.brettnamba.DotSync.FileSystem.Application.Jobs;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;

public sealed record PushByStorageParameters(Guid StorageLocationId, string PathPrefixFilter, long UploadLimitMb)
    : IJobParameters
{
    public JobType Type => JobType.PushByStorage;

    public IReadOnlyDictionary<string, string> AsKeyValuePairs()
    {
        return new Dictionary<string, string>()
        {
            { "StorageLocationId", StorageLocationId.ToString("D") },
            { "PathPrefixFilter", PathPrefixFilter },
            { "UploadLimitMb", UploadLimitMb.ToString() },
        }.AsReadOnly();
    }
}