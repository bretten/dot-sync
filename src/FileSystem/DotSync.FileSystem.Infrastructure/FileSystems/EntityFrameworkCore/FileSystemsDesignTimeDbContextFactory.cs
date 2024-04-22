using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;

[ExcludeFromCodeCoverage]
public sealed class FileSystemsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<FileSystemsDbContext>
{
    public FileSystemsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FileSystemsDbContext>();
        optionsBuilder.UseNpgsql();
        return new FileSystemsDbContext(optionsBuilder.Options);
    }
}