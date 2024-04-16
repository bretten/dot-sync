using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;
using File = com.brettnamba.DotSync.FileSystem.Domain.Files.Entities.File;

namespace com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Services;

public sealed class SyncService
{
    public void Sync(StorageLocation source, StorageLocation destination)
    {
        foreach (var sourceFile in source.Files)
        {
            var matchingDestinationFile = destination.Files.Where(x => x.Id == sourceFile.Id)
                .ToList();
            if (matchingDestinationFile.Count > 1)
            {
                throw new DuplicateDestinationFileException($"Duplicate destination file: {sourceFile.Path}");
            }

            if (matchingDestinationFile.Count != 1)
            {
                Push(sourceFile, destination);
            }
        }
    }

    private void Push(File file, StorageLocation destinationLocation)
    {
    }

    public sealed class DuplicateDestinationFileException(string? message) : Exception;
}