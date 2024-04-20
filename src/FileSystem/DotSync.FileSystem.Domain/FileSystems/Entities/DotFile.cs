using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

public sealed class DotFile
{
    public Guid Id { get; }

    public FileSystemPath Path { get; private set; }

    public FileSha256Checksum Sha256Checksum { get; }

    public bool IsVerified { get; private set; }

    public DateTime UpdatedAt { get; }

    public DateTime CreatedAt { get; }

    public DotFile(Guid id, FileSystemPath path, FileSha256Checksum sha256Checksum)
    {
        Id = id;
        Path = path;
        Sha256Checksum = sha256Checksum;
    }

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