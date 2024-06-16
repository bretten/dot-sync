using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileStorage.EntityFrameworkCore;

public sealed class EntityFrameworkCoreFileStorageRepository : IFileStorageRepository
{
    private readonly IDbContextFactory<FileStorageDbContext> _dbContextFactory;
    private readonly FileStorageDbContext _dbContext;
    private readonly IClock _clock;

    public EntityFrameworkCoreFileStorageRepository(IDbContextFactory<FileStorageDbContext> dbContextFactory,
        FileStorageDbContext dbContext, IClock clock)
    {
        _dbContextFactory = dbContextFactory;
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task Add(DotFile file)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        file.LastSync = _clock.GetUtcNow();
        await dbContext.AddAsync(file);
        await dbContext.SaveChangesAsync();
    }

    public async Task Update(DotFile file)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        file.LastSync = _clock.GetUtcNow();
        dbContext.Update(file);
        await dbContext.SaveChangesAsync();
    }

    public async Task<DotFile?> GetFileByChecksum(FileSha256Checksum checksum)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Files.FirstOrDefaultAsync(x => x.Sha256Checksum == checksum);
    }

    public async Task<DotFile?> GetFileByPath(FileSystemPath path)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Files.FirstOrDefaultAsync(x => x.Path == path);
    }

    public async Task SetAllAsUnverified(StorageLocation storageLocation)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.StoredFiles.Where(x => x.StorageLocationId == storageLocation.Id).ExecuteDeleteAsync();
    }

    public async Task<IEnumerable<DotFile>> GetUnverifiedFiles()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Files.Where(x => !x.IsVerified).ToListAsync();
    }

    public async Task Add(StorageLocation storageLocation)
    {
        await _dbContext.AddAsync(storageLocation);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Update(StorageLocation storageLocation)
    {
        _dbContext.Update(storageLocation);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<StorageLocation?> GetByTypeAndPath(StorageLocationType type, FileSystemPath path)
    {
        return await _dbContext.StorageLocationsWithHistoricalStatistics()
            .FirstOrDefaultAsync(x => x.Type == type && x.Path == path);
    }
}