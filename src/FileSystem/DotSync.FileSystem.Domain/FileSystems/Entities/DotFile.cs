using com.brettnamba.DotSync.Common.Domain.SeedWork;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

/// <summary>
/// Represents a file
/// </summary>
public sealed class DotFile : Entity
{
    /// <summary>
    /// Relative path to the file
    /// </summary>
    public FileSystemPath Path { get; private set; }

    /// <summary>
    /// Checksum of the file
    /// </summary>
    public FileSha256Checksum Sha256Checksum { get; }

    /// <summary>
    /// Size of the file
    /// </summary>
    public long Size { get; }

    /// <summary>
    /// When the file itself was created
    /// </summary>
    public DateTime FileCreation { get; }

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
    /// Updates the path of the file
    /// </summary>
    public void UpdatePath(FileSystemPath newPath)
    {
        Path = newPath;
    }

    public DotFile(Guid id, FileSystemPath path, FileSha256Checksum sha256Checksum, long size,
        DateTime fileCreation) : base(id)
    {
        Path = path;
        Sha256Checksum = sha256Checksum;
        Size = size;
        FileCreation = fileCreation;
    }
}