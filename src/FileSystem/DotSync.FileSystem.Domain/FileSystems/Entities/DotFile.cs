using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

public sealed class DotFile(Guid id, FileSystemPath path, FileSha256Checksum sha256Checksum, long size)
{
    public Guid Id { get; } = id;

    public FileSystemPath Path { get; private set; } = path;

    public FileSha256Checksum Sha256Checksum { get; } = sha256Checksum;

    public long Size { get; } = size;

    public bool IsVerified { get; private set; }

    public DateTime UpdatedAt { get; }

    public DateTime CreatedAt { get; }

    public bool Match(DotFile otherFile)
    {
        if (Sha256Checksum.Value != otherFile.Sha256Checksum.Value)
        {
            return false;
        }

        return Path == otherFile.Path;
    }

    public void UpdatePath(string newPath)
    {
        Path = FileSystemPath.Create(newPath);
    }

    public void SetAsVerified()
    {
        IsVerified = true;
    }
}