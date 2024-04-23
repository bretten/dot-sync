using System.Data.Common;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.Files.TestClasses;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Tests.FileSystems.EntityFrameworkCore;

public class EntityFrameworkCoreFileRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    /// <summary>
    /// Configures the container options that will be used for each test
    /// </summary>
    /// <exception cref="DockerNotRunningException">Thrown if Docker is not running</exception>
    public EntityFrameworkCoreFileRepositoryIntegrationTests()
    {
        try
        {
            _container = new PostgreSqlBuilder()
                .WithName(GetType().Name + Guid.NewGuid())
                .WithUsername("postgres")
                .WithPassword("password99")
                .WithPortBinding(54326, 5432)
                .Build();
        }
        catch (ArgumentException e)
        {
            if (!e.Message.Contains("Docker is either not running or misconfigured"))
            {
                throw;
            }

            throw new DockerNotRunningException($"Docker is required to run tests for {GetType().Name}");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_File_AddsFileToDbContextSet()
    {
        // Arrange
        var fakeFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file");

        await using var connection = await GetDbConnection();
        await using var dbContext = GetDbContext(connection);
        await dbContext.Database.MigrateAsync();

        var repo = new EntityFrameworkCoreFileRepository(dbContext);

        // Act
        await repo.Add(fakeFile);
        await dbContext.SaveChangesAsync();

        // Assert
        Assert.NotNull(dbContext.Files);
        Assert.Equal(1, dbContext.Files.Count());
        Assert.Equal(fakeFile, dbContext.Files.First());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Update_File_UpdatesFileInDbContextSet()
    {
        // Arrange
        var fakeFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file");

        await using var connection = await GetDbConnection();
        await using var dbContext = GetDbContext(connection);
        await dbContext.Database.MigrateAsync();

        await dbContext.AddAsync(fakeFile);
        await dbContext.SaveChangesAsync();

        var repo = new EntityFrameworkCoreFileRepository(dbContext);

        // Act
        fakeFile.SetAsVerified();
        await repo.Update(fakeFile);
        await dbContext.SaveChangesAsync();

        // Assert
        await using var assertConnection =
            await GetDbConnection(); // Re-create the context so that the record is freshly retrieved from the database
        await using var assertDbContext = GetDbContext(assertConnection);
        await assertDbContext.Database.MigrateAsync();
        var actual = assertDbContext.Files.First();
        Assert.True(actual.IsVerified);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetFileByChecksum_Checksum_ReturnsFileByChecksum()
    {
        // Arrange
        var fakeFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file");

        await using var connection = await GetDbConnection();
        await using var dbContext = GetDbContext(connection);
        await dbContext.Database.MigrateAsync();

        await dbContext.AddAsync(fakeFile);
        await dbContext.SaveChangesAsync();

        await using var assertConnection =
            await GetDbConnection(); // Re-create the context so that the record is freshly retrieved from the database
        await using var assertDbContext = GetDbContext(assertConnection);
        await assertDbContext.Database.MigrateAsync();
        var repo = new EntityFrameworkCoreFileRepository(assertDbContext);

        // Act
        var actual = await repo.GetFileByChecksum(FileSha256Checksum.Create("file"));

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(fakeFile.Id, actual.Id);
        Assert.Equal(fakeFile.Sha256Checksum, actual.Sha256Checksum);
        Assert.Equal(fakeFile.Path, actual.Path);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetFileByPath_Path_ReturnsFileByPath()
    {
        // Arrange
        var fakeFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file");

        await using var connection = await GetDbConnection();
        await using var dbContext = GetDbContext(connection);
        await dbContext.Database.MigrateAsync();

        await dbContext.AddAsync(fakeFile);
        await dbContext.SaveChangesAsync();

        await using var assertConnection =
            await GetDbConnection(); // Re-create the context so that the record is freshly retrieved from the database
        await using var assertDbContext = GetDbContext(assertConnection);
        await assertDbContext.Database.MigrateAsync();
        var repo = new EntityFrameworkCoreFileRepository(assertDbContext);

        // Act
        var actual = await repo.GetFileByPath(FileSystemPath.Create("path/to/file.txt"));

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(fakeFile.Id, actual.Id);
        Assert.Equal(fakeFile.Sha256Checksum, actual.Sha256Checksum);
        Assert.Equal(fakeFile.Path, actual.Path);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SetAllAsUnverified_DirectoryPath_SetsAllAsUnverifiedInPath()
    {
        // Arrange
        var fakeFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file");
        var fakeFile2 = Faker.FakeFile(path: "path/to/file2.txt", checksum: "file2");

        await using var connection = await GetDbConnection();
        await using var dbContext = GetDbContext(connection);
        await dbContext.Database.MigrateAsync();

        fakeFile.SetAsVerified();
        fakeFile2.SetAsVerified();
        await dbContext.AddAsync(fakeFile);
        await dbContext.AddAsync(fakeFile2);
        await dbContext.SaveChangesAsync();

        await using var assertConnection =
            await GetDbConnection(); // Re-create the context so that the record is freshly retrieved from the database
        await using var assertDbContext = GetDbContext(assertConnection);
        await assertDbContext.Database.MigrateAsync();
        var repo = new EntityFrameworkCoreFileRepository(assertDbContext);

        // Act
        await repo.SetAllAsUnverified();

        // Assert
        var actual = assertDbContext.Files.ToList();
        Assert.Equal(2, actual.Count);
        Assert.False(actual.First(x => x.Id == fakeFile.Id).IsVerified);
        Assert.False(actual.First(x => x.Id == fakeFile2.Id).IsVerified);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetUnverifiedFiles_DirectoryPath_ReturnsUnverifiedFilesInPath()
    {
        // Arrange
        var fakeFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file");
        var fakeFile2 = Faker.FakeFile(path: "path/to/file2.txt", checksum: "file2");
        var fakeFile3 = Faker.FakeFile(path: "path/to/file3.txt", checksum: "file3");
        var fakeFile4 = Faker.FakeFile(path: "path/to/file4.txt", checksum: "file4");

        await using var connection = await GetDbConnection();
        await using var dbContext = GetDbContext(connection);
        await dbContext.Database.MigrateAsync();

        fakeFile.SetAsVerified();
        fakeFile2.SetAsVerified();
        await dbContext.AddAsync(fakeFile);
        await dbContext.AddAsync(fakeFile2);
        await dbContext.SaveChangesAsync();

        await using var assertConnection =
            await GetDbConnection(); // Re-create the context so that the record is freshly retrieved from the database
        await using var assertDbContext = GetDbContext(assertConnection);
        await assertDbContext.Database.MigrateAsync();
        var repo = new EntityFrameworkCoreFileRepository(assertDbContext);

        // Act
        var actual = await repo.GetUnverifiedFiles();

        // Assert
        Assert.NotNull(actual);
        var actualList = actual.ToList();
        Assert.Equal(2, actualList.Count);
        Assert.Single(actualList.Where(x => x.Id == fakeFile3.Id));
        Assert.Single(actualList.Where(x => x.Id == fakeFile4.Id));
    }

    public async Task InitializeAsync() => await _container.StartAsync();

    public async Task DisposeAsync() => await _container.StopAsync();

    private async Task<DbConnection> GetDbConnection()
    {
        NpgsqlConnection connection = new(_container.GetConnectionString() + ";Pooling=false;");
        await connection.OpenAsync();
        return connection;
    }

    private FileSystemsDbContext GetDbContext(DbConnection connection)
    {
        var contextOptions = new DbContextOptionsBuilder<FileSystemsDbContext>()
            .UseNpgsql(connection)
            .LogTo(Console.WriteLine)
            .Options;
        return new FileSystemsDbContext(contextOptions);
    }

    private sealed class DockerNotRunningException(string? message) : Exception(message);
}