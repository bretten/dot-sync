using System.Data.Common;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Exceptions;
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
        var fakeStorageLocation = Faker.FakeStorageLocation(path: "/path/to/storage/");

        var factory = await GetDbContextFactory();

        var repo = new EntityFrameworkCoreStorageLocationRepository(factory);

        // Act
        await repo.Add(fakeStorageLocation);

        // Assert
        var assertDbContext = await (await GetDbContextFactory()).CreateDbContextAsync();
        Assert.NotNull(assertDbContext.StorageLocations);
        Assert.Equal(1, assertDbContext.StorageLocations.Count());
        Assert.Equal(fakeStorageLocation.Id, assertDbContext.StorageLocations.First().Id);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_MainLocalStorageAlreadyExists_ThrowsException()
    {
        // Arrange
        var fakeStorageLocation = Faker.FakeStorageLocation(path: "/path/to/storage/", type: StorageLocationType.Local);

        var arrangeFactory = await GetDbContextFactory();
        var arrangeDbContext = await arrangeFactory.CreateDbContextAsync();
        await arrangeDbContext.StorageLocations.AddAsync(Faker.FakeStorageLocation(path: "/existing/main/storage/",
            type: StorageLocationType.Local));
        await arrangeDbContext.SaveChangesAsync();

        var factory = await GetDbContextFactory();

        var repo = new EntityFrameworkCoreStorageLocationRepository(factory);

        var action = async () => await repo.Add(fakeStorageLocation);

        // Act
        var actual = await Record.ExceptionAsync(action);

        // Assert
        Assert.NotNull(actual);
        Assert.IsType<MainStorageAlreadyExistsException>(actual);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Exists_NewStorageLocation_ReturnsFalse()
    {
        // Arrange
        var fakeStorageLocation = Faker.FakeStorageLocation(path: "/path/to/storage/");

        var repo = new EntityFrameworkCoreStorageLocationRepository(await GetDbContextFactory());

        // Act
        var actual = await repo.Exists(fakeStorageLocation.Type, fakeStorageLocation.Path);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Exists_ExistingStorageLocation_ReturnsTrue()
    {
        // Arrange
        var fakeStorageLocation = Faker.FakeStorageLocation(path: "/path/to/storage/");

        var arrangeFactory = await GetDbContextFactory();
        var arrangeDbContext = await arrangeFactory.CreateDbContextAsync();
        await arrangeDbContext.StorageLocations.AddAsync(fakeStorageLocation);
        await arrangeDbContext.SaveChangesAsync();

        var factory = await GetDbContextFactory();

        var repo = new EntityFrameworkCoreStorageLocationRepository(factory);

        // Act
        var actual = await repo.Exists(fakeStorageLocation.Type, fakeStorageLocation.Path);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMainStorageLocation_DoesNotExist_ThrowsException()
    {
        // Arrange
        var factory = await GetDbContextFactory();

        var repo = new EntityFrameworkCoreStorageLocationRepository(factory);

        var action = async () => await repo.GetMainStorageLocation();

        // Act
        var actual = await Record.ExceptionAsync(action);

        // Assert
        Assert.NotNull(actual);
        Assert.IsType<MainStorageDoesNotExistException>(actual);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMainStorageLocation_Exists_MainStorageReturned()
    {
        // Arrange
        var fakeStorageLocation = Faker.FakeStorageLocation(path: "/path/to/storage/", type: StorageLocationType.Local);

        var arrangeFactory = await GetDbContextFactory();
        var arrangeDbContext = await arrangeFactory.CreateDbContextAsync();
        await arrangeDbContext.StorageLocations.AddAsync(fakeStorageLocation);
        await arrangeDbContext.SaveChangesAsync();

        var factory = await GetDbContextFactory();

        var repo = new EntityFrameworkCoreStorageLocationRepository(factory);

        // Act
        var actual = await repo.GetMainStorageLocation();

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(StorageLocationType.Local, actual.Type);
        Assert.Equal(fakeStorageLocation.Id, actual.Id);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetPathInMainStorage_FilePath_PathReturnsInMainStorage()
    {
        // Arrange
        var fakeStorageLocation = Faker.FakeStorageLocation(path: "/path/to/storage/", type: StorageLocationType.Local);
        var filePath = Faker.FakeFileSystemPath("path/to/file.jpg");

        var arrangeFactory = await GetDbContextFactory();
        var arrangeDbContext = await arrangeFactory.CreateDbContextAsync();
        await arrangeDbContext.StorageLocations.AddAsync(fakeStorageLocation);
        await arrangeDbContext.SaveChangesAsync();

        var factory = await GetDbContextFactory();

        var repo = new EntityFrameworkCoreStorageLocationRepository(factory);

        // Act
        var actual = await repo.GetPathInMainStorage(filePath);

        // Assert
        Assert.Equal("/path/to/storage/path/to/file.jpg", actual);
    }

    public async Task InitializeAsync() => await _container.StartAsync();

    public async Task DisposeAsync() => await _container.StopAsync();

    private async Task<DbConnection> GetDbConnection()
    {
        NpgsqlConnection connection = new(_container.GetConnectionString() + ";Pooling=false;");
        await connection.OpenAsync();
        return connection;
    }

    private async Task<FileSystemsDbContext> GetDbContext()
    {
        var connection = await GetDbConnection();
        var contextOptions = new DbContextOptionsBuilder<FileSystemsDbContext>()
            .UseNpgsql(connection)
            .LogTo(Console.WriteLine)
            .Options;
        var dbContext = new FileSystemsDbContext(contextOptions);
        await dbContext.Database.MigrateAsync();
        return dbContext;
    }

    private Task<IDbContextFactory<FileSystemsDbContext>> GetDbContextFactory()
    {
        var factory = new Mock<IDbContextFactory<FileSystemsDbContext>>();
        factory.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .Returns(async () => await GetDbContext());
        return Task.FromResult(factory.Object);
    }

    private sealed class DockerNotRunningException(string? message) : Exception(message);
}