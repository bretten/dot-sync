using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileStorage.EntityFrameworkCore;

public sealed class FileStorageDbContext(DbContextOptions<FileStorageDbContext> options) : DbContext(options)
{
    public DbSet<DotFile> Files { get; private init; } = null!;
    public DbSet<StorageLocation> StorageLocations { get; private init; } = null!;
    public DbSet<StoredFile> StoredFiles { get; private init; } = null!;

    /// <summary>
    /// Returns the <see cref="StorageLocation"/> DB set with <see cref="HistoricalStorageStatistics"/>s included
    /// </summary>
    /// <returns><see cref="IQueryable"/> for <see cref="StorageLocation"/></returns>
    public IQueryable<StorageLocation> StorageLocationsWithHistoricalStatistics()
    {
        return StorageLocations
            .Include(e => e.HistoricalStorageStatistics);
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Constants.Schema);
        modelBuilder.ApplyConfiguration(new DotFileEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new StorageLocationEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new HistoricalStorageStatisticsEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new StoredFileEntityTypeConfiguration());
    }
}