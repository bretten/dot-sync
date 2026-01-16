using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Contracts;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;

public sealed record PushParameters(
    StorageLocation Source,
    string? PathPrefix,
    StorageLocation Destination) : IJobParameters
{
    public JobType Type => JobType.Push;

    public IReadOnlyDictionary<string, string> AsKeyValuePairs()
    {
        return new Dictionary<string, string>()
        {
            { "Source", $"{Source.Type} - {Source.Path.Value}" },
            { "Path prefix", PathPrefix! },
            { "Destination", $"{Destination.Type} - {Destination.Path.Value}" }
        }.AsReadOnly();
    }
}