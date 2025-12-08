using System.ComponentModel;
using com.brettnamba.DotSync.Common.Extensions;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;

public sealed class StorageLocationEntityTypeConfiguration : IEntityTypeConfiguration<StorageLocation>
{
    public void Configure(EntityTypeBuilder<StorageLocation> builder)
    {
        builder.ToTable(Constants.StorageLocations.TableName, Constants.Schema);

        builder.HasKey(e => e.Id)
            .HasName(Constants.StorageLocations.Keys.PrimaryKey);

        builder.HasIndex(e => e.Path, Constants.StorageLocations.Indexes.PathIndex)
            .IsUnique();

        var columnOrder = 0;

        builder.Property(e => e.Id)
            .IsRequired()
            .HasColumnType("uuid")
            .HasColumnName(Constants.StorageLocations.Id)
            .HasColumnOrder(columnOrder++);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasColumnType("varchar(12)")
            .HasColumnName(Constants.StorageLocations.Type)
            .HasColumnOrder(columnOrder++)
            .HasConversion(v => v.GetDisplayName(),
                v => (StorageLocationType)TypeDescriptor.GetConverter(typeof(StorageLocationType)).ConvertFrom(v)!);

        builder.Property(e => e.Path)
            .IsRequired()
            .HasColumnType("text")
            .HasColumnName(Constants.StorageLocations.Path)
            .HasColumnOrder(columnOrder++)
            .HasConversion(v => v.Value,
                v => FileSystemPath.Create(v));
    }
}