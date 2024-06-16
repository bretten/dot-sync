using System.Data.Common;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.Files.TestClasses;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileStorage.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;
using Testcontainers.PostgreSql;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Tests.FileStorage.EntityFrameworkCore;

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

        var repo = new EntityFrameworkCoreFileStorageRepository(await GetDbContextFactory(), dbContext, Mock.Of<IClock>());

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
    public async Task Update_File_UpdatesFileInDbContextSet()
    {
        // Arrange
        var fakeFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file");

        await using var connection = await GetDbConnection();
        await using var dbContext = GetDbContext(connection);
        await dbContext.Database.MigrateAsync();

        await dbContext.AddAsync(fakeFile);
        await dbContext.SaveChangesAsync();

        var repo = new EntityFrameworkCoreFileStorageRepository(await GetDbContextFactory(), dbContext, Mock.Of<IClock>());

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
        var repo = new EntityFrameworkCoreFileStorageRepository(await GetDbContextFactory(), dbContext, Mock.Of<IClock>());

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
        var repo = new EntityFrameworkCoreFileStorageRepository(await GetDbContextFactory(), dbContext, Mock.Of<IClock>());

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
    public async Task SetAllAsUnverified_VerifiedFiles_SetsAllAsUnverified()
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
        var repo = new EntityFrameworkCoreFileStorageRepository(await GetDbContextFactory(), dbContext, Mock.Of<IClock>());

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
    public async Task GetUnverifiedFiles_MixOfVerifiedAndUnverifiedFiles_ReturnsUnverifiedFiles()
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
        await dbContext.AddAsync(fakeFile3);
        await dbContext.AddAsync(fakeFile4);
        await dbContext.SaveChangesAsync();

        await using var assertConnection =
            await GetDbConnection(); // Re-create the context so that the record is freshly retrieved from the database
        await using var assertDbContext = GetDbContext(assertConnection);
        await assertDbContext.Database.MigrateAsync();
        var repo = new EntityFrameworkCoreFileStorageRepository(await GetDbContextFactory(), dbContext, Mock.Of<IClock>());

        // Act
        var actual = await repo.GetUnverifiedFiles();

        // Assert
        Assert.NotNull(actual);
        var actualList = actual.ToList();
        Assert.Equal(2, actualList.Count);
        Assert.Single(actualList.Where(x => x.Id == fakeFile3.Id));
        Assert.Single(actualList.Where(x => x.Id == fakeFile4.Id));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_StorageLocation_AddsStorageLocationToDbContextSet()
    {
        // Arrange
        var fakeStorageLocation = Faker.FakeStorageLocation(path: "path/to/storage", fileCount: 1, size: 2);

        await using var connection = await GetDbConnection();
        await using var dbContext = GetDbContext(connection);
        await dbContext.Database.MigrateAsync();

        var repo = new EntityFrameworkCoreFileStorageRepository(await GetDbContextFactory(), dbContext, Mock.Of<IClock>());

        // Act
        await repo.Add(fakeStorageLocation);
        await dbContext.SaveChangesAsync();

        // Assert
        Assert.NotNull(dbContext.StorageLocations);
        Assert.Equal(1, dbContext.StorageLocations.Count());
        Assert.Equal(fakeStorageLocation, dbContext.StorageLocations.First());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Update_StorageLocation_UpdatesStorageLocationInDbContextSet()
    {
        // Arrange
        var fakeStorageLocation = Faker.FakeStorageLocation(path: "path/to/storage", fileCount: 1, size: 2);

        await using var connection = await GetDbConnection();
        await using var dbContext = GetDbContext(connection);
        await dbContext.Database.MigrateAsync();

        await dbContext.AddAsync(fakeStorageLocation);
        await dbContext.SaveChangesAsync();

        var repo = new EntityFrameworkCoreFileStorageRepository(await GetDbContextFactory(), dbContext, Mock.Of<IClock>());

        // Act
        fakeStorageLocation.UpdateStatistics(fileCount: 10, storageSize: 20,
            new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero));
        await repo.Update(fakeStorageLocation);
        await dbContext.SaveChangesAsync();

        // Assert
        await using var assertConnection =
            await GetDbConnection(); // Re-create the context so that the record is freshly retrieved from the database
        await using var assertDbContext = GetDbContext(assertConnection);
        await assertDbContext.Database.MigrateAsync();
        var actual = assertDbContext.StorageLocationsWithHistoricalStatistics().First();
        Assert.Equal(10, actual.StorageStatistics.FileCount);
        Assert.Equal(20, actual.StorageStatistics.Size);
        Assert.Single(actual.HistoricalStorageStatistics);
        Assert.Equal(new HistoricalStorageStatistics(
                new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero),
                new StorageStatistics(fileCount: 10, size: 20)
            ),
            actual.HistoricalStorageStatistics.First());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetByTypeAndPath_TypeAndPath_ReturnsStorageLocation()
    {
        // Arrange
        var fakeStorageLocation = Faker.FakeStorageLocation(path: "path/to/storage", fileCount: 1, size: 2);

        await using var connection = await GetDbConnection();
        await using var dbContext = GetDbContext(connection);
        await dbContext.Database.MigrateAsync();

        await dbContext.AddAsync(fakeStorageLocation);
        await dbContext.SaveChangesAsync();

        await using var assertConnection =
            await GetDbConnection(); // Re-create the context so that the record is freshly retrieved from the database
        await using var assertDbContext = GetDbContext(assertConnection);
        await assertDbContext.Database.MigrateAsync();
        var repo = new EntityFrameworkCoreFileStorageRepository(await GetDbContextFactory(), dbContext, Mock.Of<IClock>());

        // Act
        var actual = await repo.GetByTypeAndPath(StorageLocationType.Local,
            FileSystemPath.Create(@"path\to\storage", replaceBackslashes: OperatingSystem.IsWindows()));

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(fakeStorageLocation.Id, actual.Id);
        Assert.Equal(fakeStorageLocation.Path, actual.Path);
        Assert.Equal(fakeStorageLocation.StorageStatistics.FileCount, actual.StorageStatistics.FileCount);
        Assert.Equal(fakeStorageLocation.StorageStatistics.Size, actual.StorageStatistics.Size);
    }

    public async Task InitializeAsync() => await _container.StartAsync();

    public async Task DisposeAsync() => await _container.StopAsync();

    private async Task<DbConnection> GetDbConnection()
    {
        NpgsqlConnection connection = new(_container.GetConnectionString() + ";Pooling=false;");
        await connection.OpenAsync();
        return connection;
    }

    private FileStorageDbContext GetDbContext(DbConnection connection)
    {
        var contextOptions = new DbContextOptionsBuilder<FileStorageDbContext>()
            .UseNpgsql(connection)
            .LogTo(Console.WriteLine)
            .Options;
        return new FileStorageDbContext(contextOptions);
    }

    private async Task<IDbContextFactory<FileStorageDbContext>> GetDbContextFactory()
    {
        var connection = await GetDbConnection();
        var context = GetDbContext(connection);
        var stubDbContextFactory = new Mock<IDbContextFactory<FileStorageDbContext>>();
        stubDbContextFactory.Setup(x => x.CreateDbContextAsync(CancellationToken.None))
            .ReturnsAsync(context);
        return stubDbContextFactory.Object;
    }

    private sealed class DockerNotRunningException(string? message) : Exception(message);
}