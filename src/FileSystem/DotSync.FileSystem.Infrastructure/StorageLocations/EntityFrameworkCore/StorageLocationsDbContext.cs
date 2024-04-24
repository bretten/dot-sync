using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.StorageLocations.EntityFrameworkCore;

public sealed class StorageLocationsDbContext(DbContextOptions<StorageLocationsDbContext> options) : DbContext(options)
{
    public DbSet<StorageLocation> StorageLocations { get; private init; } = null!;

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
        modelBuilder.ApplyConfiguration(new StorageLocationEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new HistoricalStorageStatisticsEntityTypeConfiguration());
    }
}