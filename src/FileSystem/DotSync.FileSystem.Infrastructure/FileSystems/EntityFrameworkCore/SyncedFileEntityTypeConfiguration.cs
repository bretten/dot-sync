using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;

public sealed class SyncedFileEntityTypeConfiguration : IEntityTypeConfiguration<SyncedFile>
{
    public void Configure(EntityTypeBuilder<SyncedFile> builder)
    {
        builder.ToTable(Constants.SyncedFiles.TableName, Constants.Schema);

        builder.HasKey(e => new { e.FileId, e.StorageLocationId })
            .HasName(Constants.SyncedFiles.Keys.PrimaryKey);

        // The primary key is a composite key of FileId and StorageLocationId. Since FileId is the leading column, it does not need an index applied
        // But the second column in the composite index still needs an index
        builder.HasIndex(e => e.StorageLocationId, Constants.SyncedFiles.Indexes.StorageLocation);

        var columnOrder = 0;

        builder.Property(e => e.FileId)
            .IsRequired()
            .HasColumnType("uuid")
            .HasColumnName(Constants.SyncedFiles.FileId)
            .HasColumnOrder(columnOrder++);

        builder.Property(e => e.StorageLocationId)
            .IsRequired()
            .HasColumnType("uuid")
            .HasColumnName(Constants.SyncedFiles.StoreLocationId)
            .HasColumnOrder(columnOrder++);

        builder.Property(e => e.LastSync)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName(Constants.SyncedFiles.LastSync)
            .HasColumnOrder(columnOrder++);
    }
}