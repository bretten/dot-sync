using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.StorageLocations.EntityFrameworkCore;

public sealed class EntityFrameworkCoreStorageLocationRepository(FileSystemsDbContext dbContext)
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
        return await dbContext.StorageLocations
            .FirstOrDefaultAsync(x => x.Type == type && x.Path == path);
    }

    public async Task<IEnumerable<StorageLocation>> GetAll()
    {
        return await dbContext.StorageLocations.ToListAsync();
    }
}