namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

/// <summary>
/// Represents a <see cref="DotFile"/> that has been synced to a <see cref="StorageLocation"/>.
/// Or, the entity that represents the join table between the many-to-many relationship
/// </summary>
public sealed class SyncedFile
{
    /// <summary>
    /// File ID
    /// </summary>
    public Guid FileId { get; }

    /// <summary>
    /// Storage Location ID
    /// </summary>
    public Guid StorageLocationId { get; }

    /// <summary>
    /// The last time this file was synced with the system
    /// </summary>
    public DateTimeOffset LastSync { get; init; }

    /// <summary>
    /// Navigation property to <see cref="DotFile"/>
    /// </summary>
    public DotFile File { get; set; } = null!;

    /// <summary>
    /// Navigation property to <see cref="StorageLocation"/>
    /// </summary>
    public StorageLocation StorageLocation { get; set; } = null!;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fileId">File ID</param>
    /// <param name="storageLocationId">Storage Location ID</param>
    public SyncedFile(Guid fileId, Guid storageLocationId)
    {
        FileId = fileId;
        StorageLocationId = storageLocationId;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public SyncedFile()
    {
    }
}