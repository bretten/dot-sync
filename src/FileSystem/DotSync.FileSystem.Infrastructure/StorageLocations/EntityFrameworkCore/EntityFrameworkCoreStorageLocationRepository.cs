using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Repositories;
using Microsoft.EntityFrameworkCore;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.StorageLocations.EntityFrameworkCore;

public sealed class EntityFrameworkCoreStorageLocationRepository(IDbContextFactory<StorageLocationsDbContext> dbContextFactory)
    : IStorageLocationRepository
{
    public async Task Add(StorageLocation storageLocation)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.AddAsync(storageLocation);
        await dbContext.SaveChangesAsync();
    }

    public async Task Update(StorageLocation storageLocation)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.Update(storageLocation);
        await dbContext.SaveChangesAsync();
    }

    public async Task<StorageLocation?> GetByTypeAndPath(StorageLocationType type, FileSystemPath path)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.StorageLocationsWithHistoricalStatistics()
            .FirstOrDefaultAsync(x => x.Type == type && x.Path == path);
    }
}