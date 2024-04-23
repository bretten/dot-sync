using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Repositories;
using Microsoft.EntityFrameworkCore;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.StorageLocations.EntityFrameworkCore;

public sealed class EntityFrameworkCoreStorageLocationRepository(StorageLocationsDbContext dbContext)
    : IStorageLocationRepository
{
    public async Task Add(StorageLocation storageLocation)
    {
        await dbContext.AddAsync(storageLocation);
        await dbContext.SaveChangesAsync();
    }

    public async Task Update(StorageLocation storageLocation)
    {
        dbContext.Update(storageLocation);
        await dbContext.SaveChangesAsync();
    }

    public async Task<StorageLocation?> GetByTypeAndPath(StorageLocationType type, FileSystemPath path)
    {
        return await dbContext.StorageLocationsWithHistoricalStatistics()
            .FirstOrDefaultAsync(x => x.Type == type && x.Path == path);
    }
}