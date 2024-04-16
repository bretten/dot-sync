using File = com.brettnamba.DotSync.FileSystem.Domain.Files.Entities.File;

namespace com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;

public sealed class StorageLocation
{
    public Guid Id { get; }

    public string Type { get; }

    public bool IsSource { get; }

    public IEnumerable<File> Files { get; }
}