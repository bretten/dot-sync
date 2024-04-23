using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.StorageLocations.EntityFrameworkCore;

[ExcludeFromCodeCoverage]
public sealed class StorageLocationsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<StorageLocationsDbContext>
{
    public StorageLocationsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<StorageLocationsDbContext>();
        optionsBuilder.UseNpgsql();
        return new StorageLocationsDbContext(optionsBuilder.Options);
    }
}