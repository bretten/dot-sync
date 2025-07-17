using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;

public sealed record PushParameters(
    string? SourceRootPath,
    string? SourcePushPath,
    StorageLocationType DestinationType,
    string? DestinationPath) : IJobParameters
{
    public string JobId => "Push";
}