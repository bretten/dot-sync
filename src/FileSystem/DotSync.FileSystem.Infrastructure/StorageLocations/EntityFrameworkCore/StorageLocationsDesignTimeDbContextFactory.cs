using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.StorageLocations.EntityFrameworkCore;

[ExcludeFromCodeCoverage]
public sealed class StorageLocationsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<StorageLocationsDbContext>
{
    public StorageLocationsDbContext CreateDbContext(string[] args)
    {
        if (args.Length < 1)
        {
            throw new ArgumentException("Please specify the connection string as the first argument");
        }
        var optionsBuilder = new DbContextOptionsBuilder<StorageLocationsDbContext>();
        optionsBuilder.UseNpgsql(args[0]);
        return new StorageLocationsDbContext(optionsBuilder.Options);
    }
}