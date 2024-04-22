using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

/// <summary>
/// Represents a file
/// </summary>
/// <param name="id">ID of the file</param>
/// <param name="path">Relative path to the file</param>
/// <param name="sha256Checksum">Checksum of the file</param>
/// <param name="size">Size of the file</param>
public sealed class DotFile(Guid id, FileSystemPath path, FileSha256Checksum sha256Checksum, long size)
{
    /// <summary>
    /// ID
    /// </summary>
    public Guid Id { get; } = id;

    /// <summary>
    /// Relative path to the file
    /// </summary>
    public FileSystemPath Path { get; private set; } = path;

    /// <summary>
    /// Checksum of the file
    /// </summary>
    public FileSha256Checksum Sha256Checksum { get; } = sha256Checksum;

    /// <summary>
    /// Size of the file
    /// </summary>
    public long Size { get; } = size;

    /// <summary>
    /// True if the file has been verified to have the correct path and checksum
    /// </summary>
    public bool IsVerified { get; private set; }

    /// <summary>
    /// Last update
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// When it was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Checks if a file matches another file
    /// </summary>
    /// <param name="otherFile">The other file</param>
    /// <returns>True if they match, otherwise false</returns>
    public bool Match(DotFile otherFile)
    {
        if (Sha256Checksum.Value != otherFile.Sha256Checksum.Value)
        {
            return false;
        }

        return Path == otherFile.Path;
    }

    /// <summary>
    /// Updates the path of the file
    /// </summary>
    public void UpdatePath(string newPath)
    {
        Path = FileSystemPath.Create(newPath);
    }

    /// <summary>
    /// Sets the file as verified
    /// </summary>
    public void SetAsVerified()
    {
        IsVerified = true;
    }
}