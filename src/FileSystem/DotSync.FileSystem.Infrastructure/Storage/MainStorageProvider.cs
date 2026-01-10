using com.brettnamba.DotSync.FileSystem.Application.Storage;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Storage;

public sealed class MainStorageProvider : IMainStorageProvider
{
    private readonly IDbContextFactory<FileSystemsDbContext> _dbContextFactory;

    private static FileSystemPath? _mainStoragePath;
    private static FileSystemPath? _defaultCloudStoragePath;

    public MainStorageProvider(IDbContextFactory<FileSystemsDbContext> dbContextFactory)
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

        _mainStoragePath = await GetMainLocalStoragePath();
        return _mainStoragePath.Value;
    }

    /// <inheritdoc />
    public async Task<FileSystemPath> GetDefaultCloudStoragePath()
    {
        if (_defaultCloudStoragePath != null)
        {
            return _defaultCloudStoragePath.Value;
        }

        _defaultCloudStoragePath = await GetMainCloudStoragePath();
        return _defaultCloudStoragePath.Value;
    }

    /// <inheritdoc />
    public async Task<FileSystemPath> GetFileFullLocalPath(FileSystemPath filePath)
    {
        var mainStoragePath = await GetMainStoragePath();
        return FileSystemPath.Create(Path.Combine(mainStoragePath.Value, filePath.Value));
    }

    private async Task<FileSystemPath> GetMainLocalStoragePath()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var defaultStorage = dbContext.StorageLocations.FirstOrDefault(x => x.Type == StorageLocationType.Local);
        if (defaultStorage == null) throw new NullReferenceException("No local storage location found");
        return defaultStorage.Path;
    }

    private async Task<FileSystemPath> GetMainCloudStoragePath()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var defaultStorage = dbContext.StorageLocations.FirstOrDefault(x => x.Type == StorageLocationType.AmazonS3);
        if (defaultStorage == null) throw new NullReferenceException("No cloud storage location found");
        return defaultStorage.Path;
    }
}