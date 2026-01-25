using com.brettnamba.DotSync.FileSystem.Application.Storage;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Exceptions;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;

public sealed class EntityFrameworkCoreStorageLocationRepository
    : IStorageLocationRepository
{
    private readonly IDbContextFactory<FileSystemsDbContext> _dbContextFactory;
    private readonly IMainStorageProvider _mainStorageProvider;

    public EntityFrameworkCoreStorageLocationRepository(IDbContextFactory<FileSystemsDbContext> dbContextFactory,
        IMainStorageProvider mainStorageProvider)
    {
        _dbContextFactory = dbContextFactory;
        _mainStorageProvider = mainStorageProvider;
    }

    public async Task Add(StorageLocation storageLocation)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        // If a local storage was specified, make sure there is not already a local storage
        if (storageLocation.Type == StorageLocationType.Local)
        {
            var mainLocalStorage = await GetMainLocalStorage(dbContext);
            if (mainLocalStorage != null)
                throw new MainStorageAlreadyExistsException(
                    $"Main local storage location already exists: {mainLocalStorage.Path.Value}");
        }

        await dbContext.AddAsync(storageLocation);
        await dbContext.SaveChangesAsync();
    }

    public async Task Update(StorageLocation storageLocation)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        dbContext.Update(storageLocation);
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> Exists(StorageLocation storageLocation)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return null != (await dbContext.StorageLocations.FirstOrDefaultAsync(x =>
            x.Type == storageLocation.Type && x.Path == storageLocation.Path));
    }

    public async Task<StorageLocation?> GetByTypeAndPath(StorageLocationType type, StoragePath path)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.StorageLocations
            .FirstOrDefaultAsync(x => x.Type == type && x.Path == path);
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

        var mainStorage = await _mainStorageProvider.GetMainStorageLocation();
        return allStorages.Where(x => x.Id != mainStorage.Id);
    }

    private async Task<StorageLocation?> GetMainLocalStorage(FileSystemsDbContext dbContext)
    {
        return await dbContext.StorageLocations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Type == StorageLocationType.Local);
    }
}