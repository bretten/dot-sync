using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
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

    public async Task SetAllAsUnverified()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.Files.ExecuteUpdateAsync(x => x.SetProperty(e => e.IsVerified, e => false));
    }

    public async Task<IEnumerable<DotFile>> GetUnverifiedFiles()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.Files.Where(x => !x.IsVerified).ToListAsync();
    }

    /// <summary>
    /// Generates the following query:
    ///         SELECT f.id, f.file_creation, f.first_sync, f.is_verified, f.last_sync, f.path, f.sha256_checksum, f.size
    ///         FROM file_systems.files AS f
    ///         WHERE f.path::text LIKE @__path_Value_0_startswith
    /// </summary>
    public async Task<IEnumerable<DotFile>> GetFilesByPath(FileSystemPath path)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        // https://stackoverflow.com/a/63862850/1251396
        // Tricks the EF Core LINQ to SQL translator to just write WHERE Path like "value%"
        // This executes server-side
        return await dbContext.Files.Where(x => ((string)(object)x.Path).StartsWith(path.Value)).ToListAsync();
    }
}