using com.brettnamba.DotSync.FileSystem.Application.Jobs;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;

public sealed record PushByStorageParameters(Guid StorageLocationId, string PathPrefixFilter, int UploadLimitMb)
    : IJobParameters
{
    public string JobId => "PushByStorage";
}