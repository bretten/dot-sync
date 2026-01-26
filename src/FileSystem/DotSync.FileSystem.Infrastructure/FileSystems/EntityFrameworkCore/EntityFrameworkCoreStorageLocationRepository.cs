using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Exceptions;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;

public sealed class EntityFrameworkCoreStorageLocationRepository : IStorageLocationRepository
{
    private readonly IDbContextFactory<FileSystemsDbContext> _dbContextFactory;

    public EntityFrameworkCoreStorageLocationRepository(IDbContextFactory<FileSystemsDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task Add(StorageLocation storageLocation)
    {
        // If a local storage was specified, make sure there is not already a local storage
        if (storageLocation.Type == StorageLocationType.Local)
        {
            var mainLocalStorage = await GetMainLocalStorage();
            if (mainLocalStorage != null)
                throw new MainStorageAlreadyExistsException(
                    $"Main local storage location already exists: {mainLocalStorage.Path.Value}");
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.AddAsync(storageLocation);
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> Exists(StorageLocationType type, StoragePath path)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return null != (await dbContext.StorageLocations.FirstOrDefaultAsync(x => x.Type == type && x.Path == path));
    }

    /// <inheritdoc />
    public async Task<StorageLocation> GetMainStorageLocation()
    {
        var mainStorage = await GetMainLocalStorage();
        if (mainStorage == null) throw new MainStorageDoesNotExistException("No local storage location found");
        return mainStorage;
    }

    public async Task<IEnumerable<StorageLocation>> GetAll()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.StorageLocations.ToListAsync();
    }

    public async Task<IEnumerable<StorageLocation>> GetAllButMainStorage()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var allStorages = await dbContext.StorageLocations.ToListAsync();

        var mainStorage = await GetMainStorageLocation();
        return allStorages.Where(x => x.Id != mainStorage.Id);
    }

    /// <inheritdoc />
    public async Task<string> GetPathInMainStorage(FileSystemPath filePath)
    {
        var mainStorage = await GetMainLocalStorage();
        if (mainStorage == null) throw new MainStorageDoesNotExistException("No local storage location found");
        return Path.Combine(mainStorage.Path.Value, filePath.Value);
    }

    private async Task<StorageLocation?> GetMainLocalStorage()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        // Scoping issue? FirstOrDefaultAsync causes: A second operation was started on this context instance before a previous operation completed.
        return dbContext.StorageLocations.AsNoTracking()
            .FirstOrDefault(x => x.Type == StorageLocationType.Local);
    }
}