using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileStorage.EntityFrameworkCore;

public sealed class StoredFileEntityTypeConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.ToTable(Constants.StoredFiles.TableName, Constants.Schema);
    }
}