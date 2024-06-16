using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileStorage.EntityFrameworkCore;

[ExcludeFromCodeCoverage]
public sealed class FileStorageDesignTimeDbContextFactory : IDesignTimeDbContextFactory<FileStorageDbContext>
{
    public FileStorageDbContext CreateDbContext(string[] args)
    {
        if (args.Length < 1)
        {
            throw new ArgumentException("Please specify the connection string as the first argument");
        }

        var optionsBuilder = new DbContextOptionsBuilder<FileStorageDbContext>();
        optionsBuilder.UseNpgsql(args[0]);
        return new FileStorageDbContext(optionsBuilder.Options);
    }
}