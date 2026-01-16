using System.Data.Common;
using com.brettnamba.DotSync.FileSystem.Application.Storage;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.Files.TestClasses;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
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
                .WithPortBinding(54322, 5432)
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
        var fakeStorageLocation = Faker.FakeStorageLocation(path: "/path/to/storage/", fileCount: 1, size: 2);

        await using var connection = await GetDbConnection();
        await using var dbContext = GetDbContext(connection);
        await dbContext.Database.MigrateAsync();
        var factory = GetDbContextFactory(connection);

        var repo = new EntityFrameworkCoreStorageLocationRepository(factory, Mock.Of<IMainStorageProvider>());

        // Act
        await repo.Add(fakeStorageLocation);
        await dbContext.SaveChangesAsync();

        // Assert
        Assert.NotNull(dbContext.StorageLocations);
        Assert.Equal(1, dbContext.StorageLocations.Count());
        Assert.Equal(fakeStorageLocation.Id, dbContext.StorageLocations.First().Id);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetByTypeAndPath_TypeAndPath_ReturnsStorageLocation()
    {
        // Arrange
        var fakeStorageLocation = Faker.FakeStorageLocation(path: "/path/to/storage/", fileCount: 1, size: 2);

        await using var connection = await GetDbConnection();
        await using var dbContext = GetDbContext(connection);
        await dbContext.Database.MigrateAsync();
        var factory = GetDbContextFactory(connection);

        await dbContext.AddAsync(fakeStorageLocation);
        await dbContext.SaveChangesAsync();

        await using var assertConnection =
            await GetDbConnection(); // Re-create the context so that the record is freshly retrieved from the database
        await using var assertDbContext = GetDbContext(assertConnection);
        await assertDbContext.Database.MigrateAsync();
        var repo = new EntityFrameworkCoreStorageLocationRepository(factory, Mock.Of<IMainStorageProvider>());

        // Act
        var actual = await repo.GetByTypeAndPath(StorageLocationType.Local, StoragePath.Create("/path/to/storage/"));

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(fakeStorageLocation.Id, actual.Id);
        Assert.Equal(fakeStorageLocation.Path, actual.Path);
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

    private IDbContextFactory<FileSystemsDbContext> GetDbContextFactory(DbConnection connection)
    {
        var factory = new Mock<IDbContextFactory<FileSystemsDbContext>>();
        factory.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(GetDbContext(connection));
        return factory.Object;
    }

    private sealed class DockerNotRunningException(string? message) : Exception(message);
}