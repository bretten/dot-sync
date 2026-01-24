using System.Data;
using System.Diagnostics.CodeAnalysis;
using com.brettnamba.DotSync.FileSystem.Application.Files.Thumbnails;
using com.brettnamba.DotSync.FileSystem.Application.Maintenance;
using com.brettnamba.DotSync.FileSystem.Application.Storage;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Maintenance;

/// <summary>
/// Backfiller checkpoints progress using a file on the local filesystem
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class LocalCheckpointFileBackfiller : IFileBackfiller
{
    private readonly IDbContextFactory<FileSystemsDbContext> _dbContextFactory;

    private readonly LocalCheckpointFileBackfillerConfiguration _configuration;

    private readonly IThumbnailProvider _thumbnailProvider;

    private readonly NpgsqlDataSource _npgsqlDataSource;

    private readonly IMainStorageProvider _mainStorageProvider;

    private readonly IFileMetadataReader _fileMetadataReader;

    private readonly ILogger<LocalCheckpointFileBackfiller> _logger;

    private const string JobThumbnails = "thumbnails";
    private const string JobSyncedFiles = "synced_files";

    public LocalCheckpointFileBackfiller(IDbContextFactory<FileSystemsDbContext> dbContextFactory,
        IOptions<LocalCheckpointFileBackfillerConfiguration> config, IThumbnailProvider thumbnailProvider,
        NpgsqlDataSource npgsqlDataSource, IMainStorageProvider mainStorageProvider,
        IFileMetadataReader fileMetadataReader, ILogger<LocalCheckpointFileBackfiller> logger)
    {
        _dbContextFactory = dbContextFactory;
        _configuration = config.Value;
        _thumbnailProvider = thumbnailProvider;
        _npgsqlDataSource = npgsqlDataSource;
        _mainStorageProvider = mainStorageProvider;
        _fileMetadataReader = fileMetadataReader;
        _logger = logger;
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

    public async Task BackfillSyncedFiles()
    {
        var mainStorage = await _mainStorageProvider.GetMainStoragePath();
        var storageLocations = await GetStorageLocations();
        storageLocations = storageLocations.Where(x => x.Path != mainStorage).ToList();

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var lastBatch = await GetProgress(JobSyncedFiles);

        List<DotFile>? currentBatch;

        // Iterate over the files as pages/batches
        do
        {
            currentBatch = await GetBatch(dbContext, lastBatch);

            foreach (var storageLocation in storageLocations)
            {
                await using var connection = await _npgsqlDataSource.OpenConnectionAsync();
                await using var transaction = await connection.BeginTransactionAsync();
                await BulkInsertSyncedFiles(connection, transaction, currentBatch, storageLocation,
                    _syncedFileWriterDelegate);
            }

            // If successful for the whole batch and the batch was a full batch, update the progress
            if (!WasFullBatchProcessed(currentBatch.Count)) continue;
            lastBatch++;
            await WriteProgress(JobSyncedFiles, lastBatch);
        } while (currentBatch.Count == _configuration.BatchCount);
    }

    public async Task BackfillIncorrectDates()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        // Get files with incorrect dates
        var files = await dbContext.Files.AsNoTracking().Where(x => x.FileCreation <= new DateTime(1990, 1, 1))
            .ToListAsync();
        foreach (var file in files)
        {
            // File path on the main storage
            var fullLocalPath = await _mainStorageProvider.GetFileFullLocalPath(file.Path);
            _logger.LogInformation($"Fixing date for {fullLocalPath}");
            // Get the correct date
            var newDate = _fileMetadataReader.ReadFileCreationDate(fullLocalPath);

            // Update the row
            await using var connection = await _npgsqlDataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(FilesCreationDateUpdateCommand, connection);
            command.Parameters.Add(new NpgsqlParameter { Value = file.Id, DbType = DbType.Guid });
            command.Parameters.Add(new NpgsqlParameter { Value = newDate, DbType = DbType.DateTime2 });
            await command.ExecuteScalarAsync();
        }
    }

    private async Task BulkInsertSyncedFiles(NpgsqlConnection connection, NpgsqlTransaction transaction,
        List<DotFile> files, StorageLocation storageLocation,
        Action<List<DotFile>, StorageLocation, NpgsqlBinaryImporter> syncedFileWriterDelegate,
        CancellationToken cancellationToken = default)
    {
        // Create a temporary table to insert the rows
        await using var tempTableCmd = new NpgsqlCommand(SyncedFilesCreateTempTableCommand, connection, transaction);
        await tempTableCmd.ExecuteNonQueryAsync(cancellationToken);

        // COPY the SyncedFiles into the temporary table using a binary import
        await using var importer =
            await connection.BeginBinaryImportAsync(SyncedFileBinaryCopyCommand(SyncedFilesTempTableName),
                cancellationToken);
        syncedFileWriterDelegate(files, storageLocation, importer);
        await importer.CompleteAsync(cancellationToken);
        await importer.CloseAsync(cancellationToken);

        // Bulk update the actual SyncedFiles table with data from the temporary table
        await using var bulkUpdateCmd = new NpgsqlCommand(SyncedFilesBulkUpdateCommand, connection, transaction);
        await bulkUpdateCmd.ExecuteNonQueryAsync(cancellationToken);

        // Remove the temporary table
        await using var dropTempTableCmd = new NpgsqlCommand(SyncedFilesDropTempTableCommand, connection, transaction);
        await dropTempTableCmd.ExecuteNonQueryAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
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

    private async Task<List<StorageLocation>> GetStorageLocations()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.StorageLocations.AsNoTracking().ToListAsync();
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

    private const string SyncedFilesTempTableName = $"{Constants.SyncedFiles.TableName}_TEMP";

    /// <summary>
    /// SQL for creating a temp table for SyncedFile bulk inserts
    /// </summary>
    private const string SyncedFilesCreateTempTableCommand = $@"
        CREATE TABLE {Constants.Schema}.{SyncedFilesTempTableName}
        (
            {Constants.SyncedFiles.FileId}                uuid                        not null,
            {Constants.SyncedFiles.StoreLocationId}       uuid                        not null,
            {Constants.SyncedFiles.LastSync}              timestamp with time zone    not null,
            primary key ({Constants.SyncedFiles.FileId}, {Constants.SyncedFiles.StoreLocationId})
        );";

    /// <summary>
    /// Bulk binary import SQL for <see cref="SyncedFile"/>
    /// </summary>
    private string SyncedFileBinaryCopyCommand(string table) => $@"
        COPY {Constants.Schema}.{table} (
            {Constants.SyncedFiles.FileId},
            {Constants.SyncedFiles.StoreLocationId},
            {Constants.SyncedFiles.LastSync}
        )
        FROM STDIN (FORMAT BINARY)";

    /// <summary>
    /// Bulk update SQL for the <see cref="SyncedFile"/> table using the temporary table as the source
    /// </summary>
    private const string SyncedFilesBulkUpdateCommand = $@"
        INSERT INTO {Constants.Schema}.{Constants.SyncedFiles.TableName} (
            {Constants.SyncedFiles.FileId},
            {Constants.SyncedFiles.StoreLocationId},
            {Constants.SyncedFiles.LastSync}
        )
        SELECT
            {Constants.SyncedFiles.FileId},
            {Constants.SyncedFiles.StoreLocationId},
            {Constants.SyncedFiles.LastSync}
        FROM {Constants.Schema}.{SyncedFilesTempTableName}
        ON CONFLICT DO NOTHING;";

    /// <summary>
    /// SQL that drops the temporary table for <see cref="SyncedFile"/>
    /// </summary>
    private const string SyncedFilesDropTempTableCommand = $"DROP TABLE {Constants.Schema}.{SyncedFilesTempTableName}";

    /// <summary>
    /// Delegate for writing a <see cref="SyncedFile"/>s to the Npgsql binary importer
    /// </summary>
    private readonly Action<List<DotFile>, StorageLocation, NpgsqlBinaryImporter> _syncedFileWriterDelegate =
        (files, storageLocation, importer) =>
        {
            foreach (var file in files)
            {
                importer.StartRow();
                importer.Write(file.Id, NpgsqlDbType.Uuid);
                importer.Write(storageLocation.Id, NpgsqlDbType.Uuid);
                importer.Write(file.LastSync, NpgsqlDbType.TimestampTz);
            }
        };

    /// <summary>
    /// SQL for updating file creation date
    /// </summary>
    private const string FilesCreationDateUpdateCommand = $@"
        UPDATE {Constants.Schema}.{Constants.Files.TableName}
        SET {Constants.Files.FileCreation} = $2
        WHERE {Constants.Files.Id} = $1;";
}