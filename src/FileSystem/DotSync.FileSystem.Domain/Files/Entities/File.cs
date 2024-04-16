using com.brettnamba.DotSync.FileSystem.Domain.Files.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.Files.Entities;

public sealed class File
{
    public Guid Id { get; }

    public Uri Path { get; }

    public FileSha256Checksum Sha256Checksum { get; }

    public DateTime UpdatedAt { get; }

    public DateTime CreatedAt { get; }
}