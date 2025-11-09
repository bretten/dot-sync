using com.brettnamba.DotSync.FileSystem.Application.Storage;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;
using com.brettnamba.DotSync.FileSystem.Infrastructure.StorageLocations.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Storage;

public sealed class MainStorageProvider : IMainStorageProvider
{
    private readonly IDbContextFactory<StorageLocationsDbContext> _dbContextFactory;

    private static FileSystemPath? _mainStoragePath;

    public MainStorageProvider(IDbContextFactory<StorageLocationsDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public async Task<FileSystemPath> GetMainStoragePath()
    {
        if (_mainStoragePath != null)
        {
            return _mainStoragePath.Value;
        }

        _mainStoragePath = await GetStoragePath();
        return _mainStoragePath.Value;
    }

    private async Task<FileSystemPath> GetStoragePath()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var defaultStorage = dbContext.StorageLocations.FirstOrDefault(x => x.Type == StorageLocationType.Local);
        if (defaultStorage == null) throw new NullReferenceException("No local storage location found");
        return defaultStorage.Path;
    }
}