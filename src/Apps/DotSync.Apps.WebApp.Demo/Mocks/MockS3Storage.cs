using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace DotSync.Apps.WebApp.Demo.Mocks;

public sealed class MockS3Storage
{
    /// <summary>
    /// Tracks <see cref="DotFile"/>s that have been uploaded to destination <see cref="StorageLocation"/>s.
    /// The keys are the IDs of the storage location and the <see cref="HashSet{Guid}"/> is a collection of files
    /// that have already been uploaded to that storage location
    /// </summary>
    private readonly Dictionary<Guid, HashSet<DotFile>> _storageLocationsToUploadedFiles =
        new Dictionary<Guid, HashSet<DotFile>>();

    public List<DotFile> FilesFor(StorageLocation storageLocation)
    {
        if (!_storageLocationsToUploadedFiles.ContainsKey(storageLocation.Id))
        {
            return new List<DotFile>();
        }

        return _storageLocationsToUploadedFiles[storageLocation.Id].ToList();
    }

    public bool Exists(StorageLocation storageLocation, DotFile file)
    {
        if (!_storageLocationsToUploadedFiles.TryGetValue(storageLocation.Id, out var storageLocationFiles))
        {
            return false;
        }

        return storageLocationFiles.Contains(file);
    }

    public void AddFile(StorageLocation destination, DotFile file)
    {
        if (!_storageLocationsToUploadedFiles.ContainsKey(destination.Id))
        {
            _storageLocationsToUploadedFiles[destination.Id] = new HashSet<DotFile>();
        }

        _storageLocationsToUploadedFiles[destination.Id].Add(file);
    }
}