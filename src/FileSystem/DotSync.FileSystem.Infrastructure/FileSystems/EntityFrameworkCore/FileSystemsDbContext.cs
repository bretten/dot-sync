using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using Microsoft.EntityFrameworkCore;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;

public sealed class FileSystemsDbContext : DbContext
{
    public FileSystemsDbContext(DbContextOptions<FileSystemsDbContext> options) : base(options)
    {
    }

    public DbSet<DotFile> Files { get; private init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Constants.Schema);
        modelBuilder.ApplyConfiguration(new DotFileEntityTypeConfiguration());
    }
}