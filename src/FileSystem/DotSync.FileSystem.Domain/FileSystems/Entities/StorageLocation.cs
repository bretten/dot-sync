using com.brettnamba.DotSync.Common.Domain.SeedWork;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

/// <summary>
/// Defines a storage location for files
/// </summary>
public sealed class StorageLocation : Entity
{
    /// <summary>
    /// The type of storage location
    /// </summary>
    public StorageLocationType Type { get; }

    /// <summary>
    /// The path to the storage location
    /// </summary>
    public StoragePath Path { get; private set; }

    /// <summary>
    /// Navigation property to <see cref="DotFile"/>
    /// </summary>
    public List<DotFile> Files { get; } = [];

    /// <summary>
    /// Navigation property to <see cref="SyncedFiles"/>
    /// </summary>
    public List<SyncedFile> SyncedFiles { get; } = [];

    /// <summary>
    /// Constructor
    /// </summary>
    public StorageLocation(StorageLocationType type, StoragePath path) : base(Guid.NewGuid())
    {
        Type = type;
        Path = path;
    }
}