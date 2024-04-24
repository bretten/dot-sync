using System.Data.Common;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.StorageLocations.TestClasses;
using com.brettnamba.DotSync.FileSystem.Infrastructure.StorageLocations.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;
using Testcontainers.PostgreSql;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Tests.StorageLocations.EntityFrameworkCore;

public class EntityFrameworkCoreStorageLocationRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    /// <summary>
    /// Configures the container options that will be used for each test
    /// </summary>
    /// <exception cref="DockerNotRunningException">Thrown if Docker is not running</exception>
    public EntityFrameworkCoreStorageLocationRepositoryIntegrationTests()
    {
        try
        {
            _container = new PostgreSqlBuilder()
                .WithName(GetType().Name + Guid.NewGuid())
                .WithUsername("postgres")
                .WithPassword("password99")
                .WithPortBinding(54327, 5432)
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
    public async Task Add_StorageLocation_AddsStorageLocationToDbContextSet()
    {
        // Arrange
        var fakeStorageLocation = Faker.FakeStorageLocation(path: "path/to/storage", fileCount: 1, size: 2);

        await using var connection = await GetDbConnection();
        await using var dbContext = GetDbContext(connection);
        await dbContext.Database.MigrateAsync();

        var repo = new EntityFrameworkCoreStorageLocationRepository(await GetDbContextFactory());

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

        var repo = new EntityFrameworkCoreStorageLocationRepository(await GetDbContextFactory());

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
                new StorageStatistics(fileCount: 1, size: 2)
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
        var repo = new EntityFrameworkCoreStorageLocationRepository(await GetDbContextFactory());

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

    private StorageLocationsDbContext GetDbContext(DbConnection connection)
    {
        var contextOptions = new DbContextOptionsBuilder<StorageLocationsDbContext>()
            .UseNpgsql(connection)
            .LogTo(Console.WriteLine)
            .Options;
        return new StorageLocationsDbContext(contextOptions);
    }

    private async Task<IDbContextFactory<StorageLocationsDbContext>> GetDbContextFactory()
    {
        var connection = await GetDbConnection();
        var context = GetDbContext(connection);
        var stubDbContextFactory = new Mock<IDbContextFactory<StorageLocationsDbContext>>();
        stubDbContextFactory.Setup(x => x.CreateDbContextAsync(CancellationToken.None))
            .ReturnsAsync(context);
        return stubDbContextFactory.Object;
    }

    private sealed class DockerNotRunningException(string? message) : Exception(message);
}