using com.brettnamba.DotSync.FileSystem.Application.Files.Indexing;
using com.brettnamba.DotSync.FileSystem.Application.State;
using Microsoft.Extensions.Caching.Memory;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.State;

/// <summary>
/// <see cref="IEphemeralState"/> implementation using host memory <see cref="MemoryCache"/>
/// </summary>
public sealed class MemoryCacheEphemeralState : IEphemeralState
{
    private readonly IMemoryCache _memoryCache;

    private readonly IFileDirectoryIndexer _fileDirectoryIndexer;

    private const string AllFileDirectoriesKey = "AllFileDirectories";

    public MemoryCacheEphemeralState(IMemoryCache memoryCache, IFileDirectoryIndexer fileDirectoryIndexer)
    {
        _memoryCache = memoryCache;
        _fileDirectoryIndexer = fileDirectoryIndexer;
    }

    /// <inheritdoc />
    public async Task<HashSet<string>> GetAllFileDirectories()
    {
        if (!_memoryCache.TryGetValue<HashSet<string>>(AllFileDirectoriesKey, out var fileDirectories) ||
            fileDirectories == null)
        {
            var newCache = await _fileDirectoryIndexer.RetrieveAllFileDirectoryPaths();
            _memoryCache.Set(AllFileDirectoriesKey, newCache);
            fileDirectories = newCache;
        }

        return fileDirectories;
    }
}