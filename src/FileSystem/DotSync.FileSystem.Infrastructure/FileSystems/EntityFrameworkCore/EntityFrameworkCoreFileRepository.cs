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
        await dbContext.AddAsync(file);
        await dbContext.SaveChangesAsync();
    }

    public async Task Update(DotFile file)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        file.UpdatedAt = clock.GetUtcNow();
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
}