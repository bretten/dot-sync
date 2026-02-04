using System.Data.Common;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.Files.TestClasses;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Moq;
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
            _container = new PostgreSqlBuilder("postgres:17-alpine")
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

        var repo = new EntityFrameworkCoreFileRepository(await GetDbContextFactory(), Mock.Of<IClock>());

        // Act
        await repo.Add(fakeFile);
        await dbContext.SaveChangesAsync();

        // Assert
        Assert.NotNull(dbContext.Files);
        Assert.Equal(1, dbContext.Files.Count());
        Assert.Equal(fakeFile.Id, dbContext.Files.First().Id);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_SyncedFile_AddsToDbContextSet()
    {
        // Arrange
        var fakeFile1 = Faker.FakeFile(id: Faker.Guid1, path: "path/to/file.txt", checksum: "file");
        var fakeFile2 = Faker.FakeFile(id: Faker.Guid2, path: "path/to/file2.txt", checksum: "file2");
        var fakeStorage1 = Faker.FakeStorageLocation(path: "/a/path/");
        var fakeStorage2 = Faker.FakeStorageLocation(path: "/b/path/");

        await using var connection = await GetDbConnection();
        await using var dbContext = GetDbContext(connection);
        await dbContext.Database.MigrateAsync();

        dbContext.Files.Add(fakeFile1);
        dbContext.Files.Add(fakeFile2);
        dbContext.StorageLocations.Add(fakeStorage1);
        dbContext.StorageLocations.Add(fakeStorage2);
        await dbContext.SaveChangesAsync();

        var repo = new EntityFrameworkCoreFileRepository(await GetDbContextFactory(), Mock.Of<IClock>());

        // Act
        await repo.AddSyncedFile(fakeFile1.Id, fakeStorage1.Id);
        await repo.AddSyncedFile(fakeFile2.Id, fakeStorage2.Id);
        await repo.AddSyncedFile(fakeFile1.Id, fakeStorage1.Id);

        // Assert
        await using var assertConnection =
            await GetDbConnection(); // Re-create the context so that the record is freshly retrieved from the database
        await using var assertDbContext = GetDbContext(assertConnection);
        await assertDbContext.Database.MigrateAsync();
        Assert.Equal(2, assertDbContext.SyncedFiles.Count());
        Assert.NotNull(assertDbContext.SyncedFiles.FirstOrDefault(x =>
            x.FileId == fakeFile1.Id && x.StorageLocationId == fakeStorage1.Id));
        Assert.NotNull(assertDbContext.SyncedFiles.FirstOrDefault(x =>
            x.FileId == fakeFile2.Id && x.StorageLocationId == fakeStorage2.Id));
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

        var repo = new EntityFrameworkCoreFileRepository(await GetDbContextFactory(), Mock.Of<IClock>());

        // Act
        await repo.Update(fakeFile);
        await dbContext.SaveChangesAsync();

        // Assert
        await using var assertConnection =
            await GetDbConnection(); // Re-create the context so that the record is freshly retrieved from the database
        await using var assertDbContext = GetDbContext(assertConnection);
        await assertDbContext.Database.MigrateAsync();
        var actual = assertDbContext.Files.First();
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
        var repo = new EntityFrameworkCoreFileRepository(await GetDbContextFactory(), Mock.Of<IClock>());

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
        var repo = new EntityFrameworkCoreFileRepository(await GetDbContextFactory(), Mock.Of<IClock>());

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
    public async Task GetUnsyncedFiles_NoStorageId_ReturnsFilesThatDoNotHaveJoinRowWithAllStorageLocations()
    {
        /*
         * Arrange
         */
        var fakeFile1 = Faker.FakeFile(id: Faker.Guid1, path: "path/to/file.txt", checksum: "file");
        var fakeFile2 = Faker.FakeFile(id: Faker.Guid2, path: "path/to/file2.txt", checksum: "file2");
        var fakeFile3 = Faker.FakeFile(id: Faker.Guid3, path: "path/to/file3.txt", checksum: "file3");
        var fakeStorage1 = Faker.FakeStorageLocation(path: "/a/path/");
        var fakeStorage2 = Faker.FakeStorageLocation(path: "/b/path/");
        await using var connection = await GetDbConnection();
        await using var dbContext = GetDbContext(connection);
        await dbContext.Database.MigrateAsync();

        dbContext.Files.Add(fakeFile1);
        dbContext.Files.Add(fakeFile2);
        dbContext.Files.Add(fakeFile3);
        dbContext.StorageLocations.Add(fakeStorage1);
        dbContext.StorageLocations.Add(fakeStorage2);
        // File1 has join row with all storage locations
        dbContext.SyncedFiles.Add(new SyncedFile(fakeFile1.Id, fakeStorage1.Id));
        dbContext.SyncedFiles.Add(new SyncedFile(fakeFile1.Id, fakeStorage2.Id));
        // File2 has join row with partial storage locations
        dbContext.SyncedFiles.Add(new SyncedFile(fakeFile2.Id, fakeStorage2.Id));
        // File3 has no join rows
        await dbContext.SaveChangesAsync();

        var repo = new EntityFrameworkCoreFileRepository(await GetDbContextFactory(), Mock.Of<IClock>());

        /*
         * Act
         */
        var actual = (await repo.GetUnsyncedFiles()).ToList();

        /*
         * Assert
         */
        // Files2 and 3 don't have join rows with all storage locations
        Assert.Equal(2, actual.Count);
        Assert.True(actual.Count(x => x.Id == fakeFile2.Id) == 1);
        Assert.True(actual.Count(x => x.Id == fakeFile3.Id) == 1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetUnsyncedFiles_StorageId_ReturnsFilesThatDoNotHaveJoinRowWithSpecifiedStorageLocation()
    {
        /*
         * Arrange
         */
        var fakeFile1 = Faker.FakeFile(id: Faker.Guid1, path: "path/to/file.txt", checksum: "file");
        var fakeFile2 = Faker.FakeFile(id: Faker.Guid2, path: "path/to/file2.txt", checksum: "file2");
        var fakeStorage1 = Faker.FakeStorageLocation(path: "/a/path/");
        var fakeStorage2 = Faker.FakeStorageLocation(path: "/b/path/");
        var fakeStorage3 = Faker.FakeStorageLocation(path: "/c/path/");

        await using var connection = await GetDbConnection();
        await using var dbContext = GetDbContext(connection);
        await dbContext.Database.MigrateAsync();

        dbContext.Files.Add(fakeFile1);
        dbContext.Files.Add(fakeFile2);
        dbContext.StorageLocations.Add(fakeStorage1);
        dbContext.StorageLocations.Add(fakeStorage2);
        dbContext.StorageLocations.Add(fakeStorage3);
        // Storage1 has join rows with all files
        dbContext.SyncedFiles.Add(new SyncedFile(fakeFile1.Id, fakeStorage1.Id));
        dbContext.SyncedFiles.Add(new SyncedFile(fakeFile2.Id, fakeStorage1.Id));
        // Storage2 has join rows with only some files
        dbContext.SyncedFiles.Add(new SyncedFile(fakeFile2.Id, fakeStorage2.Id));
        // Storage3 has no join rows
        await dbContext.SaveChangesAsync();

        var repo = new EntityFrameworkCoreFileRepository(await GetDbContextFactory(), Mock.Of<IClock>());

        /*
         * Act
         */
        var actual1 = (await repo.GetUnsyncedFiles(fakeStorage1.Id)).ToList();
        var actual2 = (await repo.GetUnsyncedFiles(fakeStorage2.Id)).ToList();
        var actual3 = (await repo.GetUnsyncedFiles(fakeStorage3.Id)).ToList();

        /*
         * Assert
         */
        // Storage1 returns no results
        Assert.Empty(actual1);
        // Storage2 returns the join rows it is missing
        Assert.Single(actual2);
        Assert.True(actual2.Count(x => x.Id == fakeFile1.Id) == 1);
        // Storage3 returns all files since it has no join rows at all
        Assert.Equal(2, actual3.Count);
        Assert.True(actual3.Count(x => x.Id == fakeFile1.Id) == 1);
        Assert.True(actual3.Count(x => x.Id == fakeFile2.Id) == 1);
    }

    public async Task InitializeAsync() => await _container.StartAsync();

    public async Task DisposeAsync() => await _container.DisposeAsync();

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
            .EnableSensitiveDataLogging()
            .Options;
        return new FileSystemsDbContext(contextOptions);
    }

    private async Task<IDbContextFactory<FileSystemsDbContext>> GetDbContextFactory()
    {
        var connection = await GetDbConnection();
        var stubDbContextFactory = new Mock<IDbContextFactory<FileSystemsDbContext>>();
        stubDbContextFactory.Setup(x => x.CreateDbContextAsync(CancellationToken.None))
            .ReturnsAsync(() =>
                GetDbContext(
                    connection)); // new context every time to mimic db context behavior using anonymous function
        return stubDbContextFactory.Object;
    }

    private sealed class DockerNotRunningException(string? message) : Exception(message);
}