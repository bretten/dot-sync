using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.StorageLocations.EntityFrameworkCore;

public sealed class
    HistoricalStorageStatisticsEntityTypeConfiguration : IEntityTypeConfiguration<HistoricalStorageStatistics>
{
    public void Configure(EntityTypeBuilder<HistoricalStorageStatistics> builder)
    {
        builder.ToTable(Constants.HistoricalStorageStatistics.TableName, Constants.Schema);

        builder.HasOne<StorageLocation>()
            .WithMany(e => e.HistoricalStorageStatistics)
            .HasForeignKey(Constants.HistoricalStorageStatistics.StorageLocationId)
            .HasConstraintName(Constants.HistoricalStorageStatistics.StorageLocationsForeignKeyConstraint);

        builder.HasKey([
            Constants.HistoricalStorageStatistics.StorageLocationId, nameof(HistoricalStorageStatistics.DateTime)
        ]).HasName(Constants.HistoricalStorageStatistics.PrimaryKey);

        var columnOrder = 0;

        builder.Property(Constants.HistoricalStorageStatistics.StorageLocationId)
            .HasColumnOrder(columnOrder++);

        builder.Property(e => e.DateTime)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName(Constants.HistoricalStorageStatistics.Timestamp)
            .HasColumnOrder(columnOrder++);

        builder.ComplexProperty(e => e.Statistics,
            b =>
            {
                b.Property(e => e.FileCount)
                    .IsRequired()
                    .HasColumnType("bigint")
                    .HasColumnName(Constants.HistoricalStorageStatistics.FileCount)
                    .HasColumnOrder(columnOrder++);

                b.Property(e => e.Size)
                    .IsRequired()
                    .HasColumnType("bigint")
                    .HasColumnName(Constants.HistoricalStorageStatistics.Size)
                    .HasColumnOrder(columnOrder++);
            });
    }
}