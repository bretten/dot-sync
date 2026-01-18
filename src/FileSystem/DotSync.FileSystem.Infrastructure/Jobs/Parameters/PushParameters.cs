using com.brettnamba.DotSync.Common.Application.Jobs;
using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;

public sealed record PushParameters(
    StorageLocation Source,
    string? PathPrefix,
    long? UploadLimitMb,
    StorageLocation Destination) : IJobParameters
{
    public JobType Type => JobType.Push;

    public IReadOnlyDictionary<string, string> AsKeyValuePairs()
    {
        return new Dictionary<string, string>()
        {
            { "Source", $"{Source.Type} - {Source.Path.Value}" },
            { "Path prefix", PathPrefix! },
            { "Upload limit (MB)", UploadLimitMb.HasValue ? $"{UploadLimitMb.Value}MB" : "--" },
            { "Destination", $"{Destination.Type} - {Destination.Path.Value}" }
        }.AsReadOnly();
    }
}