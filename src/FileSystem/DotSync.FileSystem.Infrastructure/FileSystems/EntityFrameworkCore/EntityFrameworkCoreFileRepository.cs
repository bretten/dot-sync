using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;

public sealed class EntityFrameworkCoreFileRepository(
    IDbContextFactory<FileSystemsDbContext> dbContextFactory,
    IClock clock) : IFileRepository
{
    public async Task Add(DotFile file)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        file.LastSync = clock.GetUtcNow();
        await dbContext.AddAsync(file);
        await dbContext.SaveChangesAsync();
    }

    public async Task Update(DotFile file)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        file.LastSync = clock.GetUtcNow();
        dbContext.Update(file);
        await dbContext.SaveChangesAsync();
    }

    public async Task AddSyncedFile(Guid fileId, Guid storageLocationId)
    {
        await using var deleteDbContext = await dbContextFactory.CreateDbContextAsync();
        await deleteDbContext.SyncedFiles.Where(x => x.FileId == fileId && x.StorageLocationId == storageLocationId)
            .ExecuteDeleteAsync();

        // Need a separate db context for insert, because ExecuteDeleteAsync does not affect the EF Core change tracker
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.SyncedFiles.Add(new SyncedFile(fileId, storageLocationId)
        {
            LastSync = clock.GetUtcNow()
        });
        await dbContext.SaveChangesAsync();
    }

    public async Task<DotFile?> GetFileByChecksum(FileSha256Checksum checksum)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.Files.FirstOrDefaultAsync(x => x.Sha256Checksum == checksum);
    }

    public async Task<DotFile?> GetFileByPath(FileSystemPath path)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.Files.FirstOrDefaultAsync(x => x.Path == path);
    }

    public async Task<IEnumerable<DotFile>> GetFilesByPathPrefix(string pathPrefix)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.FilesThatStartWith(pathPrefix).ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DotFile>> GetUnsyncedFiles()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var storageLocationIds = (await dbContext.StorageLocations.ToListAsync()).Select(x => x.Id).ToList();

        // Build a predicate dynamically to make sure the resulting SQL query is (storageId = 1 or storageId = 2 ...)
        // Using LINQ .Where(x => storageIds.Contains()) will result in an ANY clause
        // NOTE: SQL will have a parameter limit, but not feasible in this app
        var predicate = PredicateBuilder.New<SyncedFile>(true);
        foreach (var id in storageLocationIds)
        {
            predicate = predicate.Or(x => x.StorageLocationId == id);
        }

        // Checks for Files that don't have the maximum number of SyncedFiles rows
        var files = from l in dbContext.Files
            join r in dbContext.SyncedFiles.Where(predicate) on l.Id equals r.FileId into gj
            from subgroup in gj.DefaultIfEmpty()
            group l by l
            into g
            where g.Count() != storageLocationIds.Count
            select g.Key;

        // Checks for Files that have no SyncedFiles rows at all
        // var files = from l in dbContext.Files
        //     join r in dbContext.SyncedFiles.Where(predicate) on l.Id equals r.FileId into gj
        //     from subgroup in gj.DefaultIfEmpty()
        //     where subgroup == null
        //     select l;
        return await files.ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DotFile>> GetUnsyncedFiles(Guid storageLocationId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var files = from l in dbContext.Files
            join r in dbContext.SyncedFiles.Where(x => x.StorageLocationId == storageLocationId) on l.Id equals r.FileId
                into gj
            from subgroup in gj.DefaultIfEmpty()
            where subgroup == null
            select l;
        return await files.ToListAsync();
    }

    public async Task<IEnumerable<DotFile>> GetUnsyncedFiles(Guid storageLocationId, string pathPrefix)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var query = dbContext.FilesThatStartWith(pathPrefix);
        query = dbContext.UnsyncedFiles(query, storageLocationId);
        return await query.ToListAsync();
    }
}