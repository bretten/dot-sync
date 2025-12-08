using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;

public sealed class FileSystemsDbContext(DbContextOptions<FileSystemsDbContext> options) : DbContext(options)
{
    public DbSet<DotFile> Files { get; private init; } = null!;

    public DbSet<StorageLocation> StorageLocations { get; private init; } = null!;

    public DbSet<SyncedFile> SyncedFiles { get; private init; } = null!;

    /// <summary>
    /// Generates the following query:
    ///         SELECT f.id, f.file_creation, f.first_sync, f.is_verified, f.last_sync, f.path, f.sha256_checksum, f.size
    ///         FROM file_systems.files AS f
    ///         WHERE f.path::text LIKE @__path_Value_0_startswith
    /// </summary>
    public IQueryable<DotFile> FilesThatStartWith(FileSystemPath path, bool includeRelated = false)
    {
        var query = includeRelated
            ? Files.Include(e => e.SyncedFiles).ThenInclude(e => e.StorageLocation).AsSplitQuery()
            : Files.AsQueryable();
        // https://stackoverflow.com/a/63862850/1251396
        // Tricks the EF Core LINQ to SQL translator to just write WHERE Path like "value%"
        // This executes server-side
        return query.Where(x => ((string)(object)x.Path).StartsWith(path.Value));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Constants.Schema);
        modelBuilder.ApplyConfiguration(new DotFileEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new StorageLocationEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SyncedFileEntityTypeConfiguration());
    }
}