using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

/// <summary>
/// Represents a file
/// </summary>
/// <param name="id">ID of the file</param>
/// <param name="path">Relative path to the file</param>
/// <param name="sha256Checksum">Checksum of the file</param>
/// <param name="size">Size of the file</param>
/// <param name="fileCreation">When the file was created (or a best estimation)</param>
public sealed class DotFile(
    Guid id,
    FileSystemPath path,
    FileSha256Checksum sha256Checksum,
    long size,
    DateTime fileCreation)
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
    /// When the file itself was created
    /// </summary>
    public DateTime FileCreation { get; } = fileCreation;

    /// <summary>
    /// True if the file has been verified to have the correct path and checksum
    /// </summary>
    public bool IsVerified { get; private set; }

    /// <summary>
    /// The last time this file was synced with the system
    /// </summary>
    public DateTimeOffset LastSync { get; set; }

    /// <summary>
    /// When the file was first synced with the system
    /// </summary>
    public DateTimeOffset FirstSync { get; init; }

    /// <summary>
    /// Navigation property to <see cref="StorageLocation"/>
    /// </summary>
    public List<StorageLocation> StorageLocations { get; } = [];

    /// <summary>
    /// Navigation property to <see cref="SyncedFile"/>
    /// </summary>
    public List<SyncedFile> SyncedFiles { get; } = [];

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
    public void UpdatePath(FileSystemPath newPath)
    {
        Path = newPath;
    }

    /// <summary>
    /// Sets the file as verified
    /// </summary>
    public void SetAsVerified()
    {
        IsVerified = true;
    }

    public DotFile(Guid id, FileSystemPath path, FileSha256Checksum sha256Checksum, long size, DateTime fileCreation,
        bool isVerified, DateTimeOffset lastSync, DateTimeOffset firstSync) : this(id, path, sha256Checksum, size,
        fileCreation)
    {
        IsVerified = isVerified;
        LastSync = lastSync;
        FirstSync = firstSync;
    }
}