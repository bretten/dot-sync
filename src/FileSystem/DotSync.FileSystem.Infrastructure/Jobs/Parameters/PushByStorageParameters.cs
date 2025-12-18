using com.brettnamba.DotSync.FileSystem.Application.Jobs;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;

public sealed record PushByStorageParameters(Guid StorageLocationId, string PathPrefixFilter, long UploadLimitMb)
    : IJobParameters
{
    public string JobId => "PushByStorage";
}