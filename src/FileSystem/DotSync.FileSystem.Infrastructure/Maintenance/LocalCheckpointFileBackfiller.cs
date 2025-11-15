using com.brettnamba.DotSync.FileSystem.Application.Files;
using com.brettnamba.DotSync.FileSystem.Application.Maintenance;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Maintenance;

/// <summary>
/// Backfiller checkpoints progress using a file on the local filesystem
/// </summary>
public sealed class LocalCheckpointFileBackfiller : IFileBackfiller
{
    private readonly IDbContextFactory<FileSystemsDbContext> _dbContextFactory;

    private readonly LocalCheckpointFileBackfillerConfiguration _configuration;

    private readonly IThumbnailProvider _thumbnailProvider;

    private const string JobThumbnails = "thumbnails";

    public LocalCheckpointFileBackfiller(IDbContextFactory<FileSystemsDbContext> dbContextFactory,
        IOptions<LocalCheckpointFileBackfillerConfiguration> config, IThumbnailProvider thumbnailProvider)
    {
        _dbContextFactory = dbContextFactory;
        _configuration = config.Value;
        _thumbnailProvider = thumbnailProvider;
    }

    public async Task BackfillThumbnails()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var lastBatch = await GetProgress(JobThumbnails);

        List<DotFile>? currentBatch;

        // Iterate over the files as pages/batches
        do
        {
            currentBatch = await GetBatch(dbContext, lastBatch);

            await Parallel.ForEachAsync(currentBatch,
                async (file, token) => { await _thumbnailProvider.GetThumbnail(file.Path); });

            // If successful for the whole batch and the batch was a full batch, update the progress
            if (!WasFullBatchProcessed(currentBatch.Count)) continue;
            lastBatch++;
            await WriteProgress(JobThumbnails, lastBatch);
        } while (currentBatch.Count == _configuration.BatchCount);
    }

    /// <summary>
    /// Gets a batch of files
    /// </summary>
    /// <param name="dbContext">DB Context</param>
    /// <param name="batch">The batch to get</param>
    /// <param name="trackChanges">True to allow EF core to track changes on the entities</param>
    /// <returns>The batch</returns>
    private async Task<List<DotFile>> GetBatch(FileSystemsDbContext dbContext, int batch, bool trackChanges = false)
    {
        var queryable = dbContext.Files
            .OrderBy(x => x.Path)
            .Skip(batch * _configuration.BatchCount).Take(_configuration.BatchCount);

        if (!trackChanges)
        {
            // No DB updates
            queryable = queryable.AsNoTracking();
        }

        return await queryable.ToListAsync();
    }

    /// <summary>
    /// Identifies if the batch processed was a full batch. If it was not, keep reprocessing it as new files will be
    /// added to this batch
    /// </summary>
    private bool WasFullBatchProcessed(int processedCount)
    {
        return processedCount >= _configuration.BatchCount;
    }

    private async Task WriteProgress(string job, int batch)
    {
        await File.WriteAllTextAsync(GetProgressFilePath(job), batch.ToString());
    }

    private async Task<int> GetProgress(string job)
    {
        var path = GetProgressFilePath(job);
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.Create(path).DisposeAsync();
        }

        var result = await File.ReadAllTextAsync(path);
        var parsed = int.TryParse(result, out var value);
        return parsed ? value : 0;
    }

    private string GetProgressFilePath(string job)
    {
        return Path.Combine(_configuration.CheckpointPath, $"{job}.txt");
    }
}