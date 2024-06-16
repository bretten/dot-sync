namespace com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Entities;

public sealed class StoredFile
{
    public Guid FileId { get; }
    public Guid StorageLocationId { get; }

    public StoredFile(Guid fileId, Guid storageLocationId)
    {
        FileId = fileId;
        StorageLocationId = storageLocationId;
    }

    public StoredFile()
    {
    }
}