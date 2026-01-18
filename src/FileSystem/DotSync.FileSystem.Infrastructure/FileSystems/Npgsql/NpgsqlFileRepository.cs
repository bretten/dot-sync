using System.Data;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using Npgsql;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.Npgsql;

public sealed class NpgsqlFileRepository : IFileRepository
{
    /// <summary>
    /// The current, active database connection
    /// </summary>
    //private NpgsqlConnection _connection;

    /// <summary>
    /// The current, active database transaction
    /// </summary>
    //private NpgsqlTransaction _transaction;
    //private TransactionScope _transaction;
    private readonly NpgsqlDataSource _dataSource;

    private readonly IClock _clock;

    public NpgsqlFileRepository(NpgsqlDataSource dataSource, IClock clock)
    {
        _dataSource = dataSource;
        _clock = clock;
    }

    // public async Task BeginTransaction()
    // {
    //     //_connection = await _dataSource.OpenConnectionAsync();
    //     //_transaction = await _connection.BeginTransactionAsync();
    //
    //     _transaction = new TransactionScope(TransactionScopeOption.Required); // Distributed transaction
    // }

    // public async Task CommitTransaction()
    // {
    //     // await _transaction.CommitAsync();
    //     // await _connection.CloseAsync();
    //     // await _transaction.DisposeAsync();
    //     // await _connection.DisposeAsync();
    //     //_transaction.Complete();
    // }

    public async Task Add(DotFile file)
    {
        file.LastSync = _clock.GetUtcNow();

        await using var connection = await _dataSource.OpenConnectionAsync();
        //connection.EnlistTransaction(Transaction.Current);
        await using var command = new NpgsqlCommand(FilesInsertCommand, connection);
        command.Parameters.Add(new NpgsqlParameter { Value = file.Id, DbType = DbType.Guid });
        command.Parameters.Add(new NpgsqlParameter { Value = file.Path.Value, DbType = DbType.String });
        command.Parameters.Add(new NpgsqlParameter { Value = file.Sha256Checksum.Value, DbType = DbType.String });
        command.Parameters.Add(new NpgsqlParameter { Value = file.Size, DbType = DbType.Int64 });
        command.Parameters.Add(new NpgsqlParameter { Value = file.FileCreation, DbType = DbType.Date });
        command.Parameters.Add(new NpgsqlParameter { Value = file.IsVerified, DbType = DbType.Boolean });
        command.Parameters.Add(new NpgsqlParameter
            { Value = file.LastSync.ToUniversalTime(), DbType = DbType.DateTimeOffset });
        command.Parameters.Add(new NpgsqlParameter
            { Value = file.FirstSync.ToUniversalTime(), DbType = DbType.DateTimeOffset });
        await command.ExecuteScalarAsync();
    }

    public Task AddSyncedFile(Guid fileId, Guid storageLocationId)
    {
        // No need to implement here yet
        throw new NotImplementedException();
    }

    public async Task Update(DotFile file)
    {
        file.LastSync = _clock.GetUtcNow();

        await using var connection = await _dataSource.OpenConnectionAsync();
        //connection.EnlistTransaction(Transaction.Current);
        await using var command = new NpgsqlCommand(FilesUpdateCommand, connection);
        command.Parameters.Add(new NpgsqlParameter { Value = file.Id, DbType = DbType.Guid });
        command.Parameters.Add(new NpgsqlParameter { Value = file.Path.Value, DbType = DbType.String });
        command.Parameters.Add(new NpgsqlParameter { Value = file.IsVerified, DbType = DbType.Boolean });
        command.Parameters.Add(new NpgsqlParameter
            { Value = file.LastSync.ToUniversalTime(), DbType = DbType.DateTimeOffset });
        await command.ExecuteScalarAsync();
    }

    public async Task<DotFile?> GetFileByChecksum(FileSha256Checksum checksum)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(FilesQueryByChecksum, connection);
        command.Parameters.Add(new NpgsqlParameter { Value = checksum.Value, DbType = DbType.String });

        await using var reader = await command.ExecuteReaderAsync();
        var result = await reader.ReadAsync();
        if (!result) return null;

        var id = reader.GetGuid(0);
        var path = reader.GetString(1);
        var checksumStr = reader.GetString(2);
        var size = reader.GetInt64(3);
        var fileCreation = reader.GetDateTime(4);
        var isVerified = reader.GetBoolean(5);
        var lastSync = reader.GetDateTime(6).ToUniversalTime();
        var firstSync = reader.GetDateTime(7).ToUniversalTime();
        await reader.CloseAsync();
        var file = new DotFile(id, FileSystemPath.Create(path), checksum, size, fileCreation)
        {
            FirstSync = firstSync,
            LastSync = lastSync
        };
        if (isVerified) file.SetAsVerified();
        return file;
    }

    public async Task<DotFile?> GetFileByPath(FileSystemPath path)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(FilesQueryByPath, connection);
        command.Parameters.Add(new NpgsqlParameter { Value = path.Value, DbType = DbType.String });

        await using var reader = await command.ExecuteReaderAsync();
        var result = await reader.ReadAsync();
        if (!result) return null;

        var id = reader.GetGuid(0);
        var pathStr = reader.GetString(1);
        var checksum = reader.GetString(2);
        var size = reader.GetInt64(3);
        var fileCreation = reader.GetDateTime(4);
        var isVerified = reader.GetBoolean(5);
        var lastSync = reader.GetDateTime(6).ToUniversalTime();
        var firstSync = reader.GetDateTime(7).ToUniversalTime();
        await reader.CloseAsync();
        var file = new DotFile(id, path, FileSha256Checksum.Create(checksum), size, fileCreation)
        {
            FirstSync = firstSync,
            LastSync = lastSync
        };
        if (isVerified) file.SetAsVerified();
        return file;
    }

    public async Task SetAllAsUnverified(string pathPrefix)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        if (pathPrefix != "")
        {
            await using var command = new NpgsqlCommand(FilesSetAsUnverifiedByPathCommand, connection);
            command.Parameters.Add(new NpgsqlParameter { Value = pathPrefix + "%", DbType = DbType.String });
            await command.ExecuteScalarAsync();
            return;
        }

        await using var command2 = new NpgsqlCommand(FilesSetAllAsUnverifiedCommand, connection);
        await command2.ExecuteScalarAsync();
    }

    public async Task<IEnumerable<DotFile>> GetUnverifiedFiles()
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(FilesQueryByUnverified, connection);

        var entries = new List<DotFile>();

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var id = reader.GetGuid(0);
            var path = reader.GetString(1);
            var checksum = reader.GetString(2);
            var size = reader.GetInt64(3);
            var fileCreation = reader.GetDateTime(4);
            var isVerified = reader.GetBoolean(5);
            var lastSync = reader.GetDateTime(6).ToUniversalTime();
            var firstSync = reader.GetDateTime(7).ToUniversalTime();
            entries.Add(new DotFile(id, FileSystemPath.Create(path), FileSha256Checksum.Create(checksum), size,
                fileCreation)
            {
                LastSync = lastSync,
                FirstSync = firstSync
            });
        }

        await reader.CloseAsync();
        return entries;
    }

    public async Task<IEnumerable<DotFile>> GetFilesByPathPrefix(string pathPrefix)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(FilesQueryByLikePath, connection);
        command.Parameters.Add(new NpgsqlParameter { Value = pathPrefix + "%", DbType = DbType.String });

        var entries = new List<DotFile>();

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var id = reader.GetGuid(0);
            var pathStr = reader.GetString(1);
            var checksum = reader.GetString(2);
            var size = reader.GetInt64(3);
            var fileCreation = reader.GetDateTime(4);
            var isVerified = reader.GetBoolean(5);
            var lastSync = reader.GetDateTime(6).ToUniversalTime();
            var firstSync = reader.GetDateTime(7).ToUniversalTime();
            var file = new DotFile(id, FileSystemPath.Create(pathStr), FileSha256Checksum.Create(checksum), size,
                fileCreation)
            {
                LastSync = lastSync,
                FirstSync = firstSync
            };
            if (isVerified) file.SetAsVerified();
            entries.Add(file);
        }

        await reader.CloseAsync();
        return entries;
    }

    public Task<IEnumerable<DotFile>> GetUnsyncedFiles()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<DotFile>> GetUnsyncedFiles(Guid storageLocationId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<DotFile>> GetUnsyncedFiles(Guid storageLocationId, string pathPrefix)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Inserts a new <see cref="DotFile"/>
    /// </summary>
    private const string FilesInsertCommand = $@"
        INSERT INTO {Constants.Schema}.{Constants.Files.TableName} (
            {Constants.Files.Id},
            {Constants.Files.Path},
            {Constants.Files.Sha256Checksum},
            {Constants.Files.Size},
            {Constants.Files.FileCreation},
            {Constants.Files.IsVerified},
            {Constants.Files.LastSync},
            {Constants.Files.FirstSync}
        )
        VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
        RETURNING {Constants.Files.Id};";

    private const string FilesUpdateCommand = $@"
        UPDATE {Constants.Schema}.{Constants.Files.TableName}
        SET (
            {Constants.Files.Path},
            {Constants.Files.IsVerified},
            {Constants.Files.LastSync}
        )
        = ($2, $3, $4)
        WHERE {Constants.Files.Id} = $1;";

    private const string FilesSetAsUnverifiedByPathCommand = $@"
        UPDATE {Constants.Schema}.{Constants.Files.TableName}
        SET {Constants.Files.IsVerified} = false
        WHERE {Constants.Files.Path} LIKE $1;";

    private const string FilesSetAllAsUnverifiedCommand = $@"
        UPDATE {Constants.Schema}.{Constants.Files.TableName}
        SET {Constants.Files.IsVerified} = false";

    private const string FilesQueryByChecksum = $@"
        SELECT * FROM {Constants.Schema}.{Constants.Files.TableName}
        WHERE {Constants.Files.Sha256Checksum} = $1;";

    private const string FilesQueryByPath = $@"
        SELECT * FROM {Constants.Schema}.{Constants.Files.TableName}
        WHERE {Constants.Files.Path} = $1;";

    private const string FilesQueryByUnverified = $@"
        SELECT * FROM {Constants.Schema}.{Constants.Files.TableName}
        WHERE {Constants.Files.IsVerified} = false;";

    private const string FilesQueryByLikePath = $@"
        SELECT * FROM {Constants.Schema}.{Constants.Files.TableName}
        WHERE {Constants.Files.Path} LIKE $1;";
}