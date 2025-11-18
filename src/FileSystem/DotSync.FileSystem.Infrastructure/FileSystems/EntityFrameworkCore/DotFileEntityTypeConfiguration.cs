using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.EntityFrameworkCore;
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

        // Results in a duplicate Path error due to the ComplexProperty below. The migrations already have the index defined, just make sure not to drop it in future migrations
        // Track progress on this issue here: https://github.com/dotnet/efcore/issues/31246
        // builder.HasIndex("path", Constants.Files.PathIndex)
        //     .IsUnique();
        builder.HasIndex(e => e.Sha256Checksum, Constants.Files.Sha256ChecksumIndex)
            .IsUnique();

        var columnOrder = 0;

        builder.Property(e => e.Id)
            .IsRequired()
            .HasColumnType("uuid")
            .HasColumnName(Constants.Files.Id)
            .HasColumnOrder(columnOrder++);

        builder.ComplexProperty(e => e.Path, b =>
        {
            b.Property(e => e.Value)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName(Constants.Files.Path)
                .HasColumnOrder(columnOrder++);
        });

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
            .HasColumnOrder(columnOrder++);

        builder.Property(e => e.FileCreation)
            .IsRequired()
            .HasColumnType("timestamp without time zone")
            .HasColumnName(Constants.Files.FileCreation)
            .HasColumnOrder(columnOrder++);

        builder.Property(e => e.IsVerified)
            .IsRequired()
            .HasColumnType("boolean")
            .HasColumnName(Constants.Files.IsVerified)
            .HasColumnOrder(columnOrder++);

        builder.Property(e => e.LastSync)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName(Constants.Files.LastSync)
            .HasColumnOrder(columnOrder++);
        //.HasComputedColumnSql("now() AT TIME ZONE 'UTC'")
        //.ValueGeneratedOnAddOrUpdate()
        //.HasValueGenerator<DateTimeOffsetValueGenerator>(); // Value generators don't work for updates: https://github.com/dotnet/efcore/issues/19765#issuecomment-770412377

        builder.Property(e => e.FirstSync)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName(Constants.Files.FirstSync)
            .HasColumnOrder(columnOrder++)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<DateTimeOffsetValueGenerator>();
    }
}