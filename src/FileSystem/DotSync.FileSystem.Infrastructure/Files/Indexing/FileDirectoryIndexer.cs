using System.Collections.Concurrent;
using System.Text;
using com.brettnamba.DotSync.FileSystem.Application.Files.Indexing;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Files.Indexing;

public sealed class FileDirectoryIndexer : IFileDirectoryIndexer
{
    private readonly IDbContextFactory<FileSystemsDbContext> _dbContextFactory;

    /// <summary>
    /// The batch size for iterating over files to discover directories
    /// </summary>
    private const int BatchSize = 1000;

    /// <summary>
    /// Dir separator
    /// Using 'Alt' separator will always be '/' on Windows or Linux: https://learn.microsoft.com/en-us/dotnet/api/system.io.path.directoryseparatorchar
    /// </summary>
    private static readonly char Ds = Path.AltDirectorySeparatorChar;

    public FileDirectoryIndexer(IDbContextFactory<FileSystemsDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public async Task<HashSet<string>> RetrieveAllFileDirectoryPaths()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var result = new ConcurrentDictionary<string, byte>();

        var batch = 0;
        var files = await GetBatch(dbContext, batch);
        do
        {
            Parallel.ForEach(files, file =>
            {
                var parentPaths = GetParentPaths(file.Path.Value);
                foreach (var parentPath in parentPaths)
                {
                    result.TryAdd(parentPath, 0);
                }
            });
            batch++;
            files = await GetBatch(dbContext, batch);
        } while (files.Count == BatchSize);

        return new HashSet<string>(result.Keys);
    }

    /// <summary>
    /// Gets a batch of files
    /// </summary>
    private async Task<List<DotFile>> GetBatch(FileSystemsDbContext dbContext, int batch)
    {
        return await dbContext.Files
            .OrderBy(x => x.Path)
            .Skip(batch * BatchSize)
            .Take(BatchSize)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Gets all parent paths for a given file path
    /// </summary>
    private List<string> GetParentPaths(string path)
    {
        // Get directory without file name
        var directoryPath = Path.GetDirectoryName(path)!;

        // Path directories
        var parts = directoryPath.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);

        // For each directory part
        var paths = new List<string>();
        for (var i = 0; i < parts.Length; i++)
        {
            // Get each possible path (each parent)
            var sb = new StringBuilder();
            for (var j = 0; j <= i; j++)
            {
                sb.Append($"{parts[j]}{Ds}");
            }

            paths.Add(sb.ToString());
        }

        return paths;
    }
}