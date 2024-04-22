using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;

public sealed class DotFileEntityTypeConfiguration : IEntityTypeConfiguration<DotFile>
{
    public void Configure(EntityTypeBuilder<DotFile> builder)
    {
        builder.ToTable(Constants.Files.TableName, Constants.Schema);

        builder.HasKey(e => e.Id)
            .HasName(Constants.Files.PrimaryKey);

        builder.HasIndex(e => e.Path, Constants.Files.PathIndex)
            .IsUnique();
        builder.HasIndex(e => e.Sha256Checksum, Constants.Files.Sha256ChecksumIndex)
            .IsUnique();

        var columnOrder = 0;

        builder.Property(e => e.Id)
            .IsRequired()
            .HasColumnType("uuid")
            .HasColumnName(Constants.Files.Id)
            .HasColumnOrder(columnOrder++);

        builder.Property(e => e.Path)
            .IsRequired()
            .HasColumnType("text")
            .HasColumnName(Constants.Files.Path)
            .HasColumnOrder(columnOrder++)
            .HasConversion(v => v.Value,
                v => FileSystemPath.Create(v));

        builder.Property(e => e.Sha256Checksum)
            .IsRequired()
            .HasColumnType("text")
            .HasColumnName(Constants.Files.Sha256Checksum)
            .HasColumnOrder(columnOrder++)
            .HasConversion(v => v.Value,
                v => FileSha256Checksum.Create(v));

        builder.Property(e => e.Size)
            .IsRequired()
            .HasColumnType("bigint")
            .HasColumnName(Constants.Files.Size)
            .HasColumnOrder(columnOrder++)
            .HasConversion(v => v,
                v => v);

        builder.Property(e => e.IsVerified)
            .IsRequired()
            .HasColumnType("boolean")
            .HasColumnName(Constants.Files.IsVerified)
            .HasColumnOrder(columnOrder++)
            .HasConversion(v => v,
                v => v);

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName(Constants.Files.UpdatedAt)
            .HasColumnOrder(columnOrder++)
            .HasConversion(v => v,
                v => v);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName(Constants.Files.CreatedAt)
            .HasColumnOrder(columnOrder++)
            .HasConversion(v => v,
                v => v);
    }
}