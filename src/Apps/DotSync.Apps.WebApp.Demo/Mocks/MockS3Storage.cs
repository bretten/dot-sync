using System.Diagnostics.CodeAnalysis;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DotSync.Apps.WebApp.Demo.Mocks;

[ExcludeFromCodeCoverage]
public sealed class MockS3Storage
{
    /// <summary>
    /// Tracks <see cref="DotFile"/>s that have been uploaded to destination <see cref="StorageLocation"/>s.
    /// The keys are the IDs of the storage location and the <see cref="HashSet{Guid}"/> is a collection of files
    /// that have already been uploaded to that storage location
    /// </summary>
    private readonly Dictionary<Guid, HashSet<DotFile>> _storageLocationsToUploadedFiles =
        new Dictionary<Guid, HashSet<DotFile>>();

    private readonly IDbContextFactory<FileSystemsDbContext> _dbContextFactory;

    public MockS3Storage(IDbContextFactory<FileSystemsDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

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

    /// <summary>
    /// Updates the state of the fake internal S3 storage to match that of the last DB state
    /// </summary>
    public async Task UpdateState()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        // Get all files that have been pushed/synced
        var syncedFiles =
            await dbContext.SyncedFiles.Include(x => x.File).Include(x => x.StorageLocation).ToListAsync();
        foreach (var syncedFile in syncedFiles)
        {
            // Because it has been synced, add it to the internal, fake S3 storage
            AddFile(syncedFile.StorageLocation, syncedFile.File);
        }
    }
}