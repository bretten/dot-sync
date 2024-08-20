using System.Data.Common;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.Files.TestClasses;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Npgsql;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;
using Testcontainers.PostgreSql;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Tests.FileSystems.Npgsql;

public class NpgsqlFileRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    /// <summary>
    /// Configures the container options that will be used for each test
    /// </summary>
    /// <exception cref="DockerNotRunningException">Thrown if Docker is not running</exception>
    public NpgsqlFileRepositoryTests()
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
    public async Task Add_DotFile_AddsToDb()
    {
        // Arrange
        var fakeFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file", size: 10, isVerified: true);

        var repo = await GetRepo();

        // Act
        await repo.Add(fakeFile);
        var actual = await repo.GetFileByChecksum(fakeFile.Sha256Checksum);

        // Assert
        Assert.NotNull(actual);
        Assert.True(Equal(fakeFile, actual));
    }

    [Fact]
    public async Task Update_DotFile_UpdatesDb()
    {
        // Arrange
        var fakeFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file", size: 10, isVerified: false);
        var fakeFile2 = Faker.FakeFile(id: Faker.Guid2, path: "path/to/file2.txt", checksum: "file2", size: 20,
            isVerified: false);

        fakeFile.SetAsVerified();
        fakeFile.UpdatePath(FileSystemPath.Create("updated/path/file.txt"));
        fakeFile.LastSync = ClockTime;

        var repo = await GetRepo();
        await repo.Add(fakeFile);
        await repo.Add(fakeFile2);

        // Act
        await repo.Update(fakeFile);
        var actual = await repo.GetFileByChecksum(fakeFile.Sha256Checksum);
        var actual2 = await repo.GetFileByChecksum(fakeFile2.Sha256Checksum);

        // Assert
        Assert.NotNull(actual);
        Assert.NotNull(actual2);
        var expectedFile = Faker.FakeFile(path: "updated/path/file.txt", checksum: "file", size: 10, isVerified: true,
            lastSync: ClockTime);
        var expectedFile2 = Faker.FakeFile(id: Faker.Guid2, path: "path/to/file2.txt", checksum: "file2", size: 20,
            isVerified: false);
        Assert.True(Equal(expectedFile, actual));
        Assert.True(Equal(expectedFile2, actual2));
    }

    [Fact]
    public async Task GetFileByChecksum_Checksum_ReturnsCorrespondingFile()
    {
        // Arrange
        var fakeFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file", size: 10, isVerified: true);

        var repo = await GetRepo();
        await repo.Add(fakeFile);

        // Act
        var actual = await repo.GetFileByChecksum(fakeFile.Sha256Checksum);

        // Assert
        Assert.NotNull(actual);
        var expectedFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file", size: 10, isVerified: true);
        Assert.True(Equal(expectedFile, actual));
    }

    [Fact]
    public async Task GetFileByPath_Path_ReturnsCorrespondingFile()
    {
        // Arrange
        var fakeFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file", size: 10, isVerified: true);

        var repo = await GetRepo();
        await repo.Add(fakeFile);

        // Act
        var actual = await repo.GetFileByPath(fakeFile.Path);

        // Assert
        Assert.NotNull(actual);
        var expectedFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file", size: 10, isVerified: true);
        Assert.True(Equal(expectedFile, actual));
    }

    [Fact]
    public async Task SetAllAsUnverified_NoPath_AllRowsSetAsUnverified()
    {
        // Arrange
        var fakeFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file", size: 10, isVerified: true);
        var fakeFile2 = Faker.FakeFile(id: Faker.Guid2, path: "path/to/file2.txt", checksum: "file2", size: 20,
            isVerified: true);

        var repo = await GetRepo();
        await repo.Add(fakeFile);
        await repo.Add(fakeFile2);

        // Act
        await repo.SetAllAsUnverified(FileSystemPath.Create(""));
        var actual = await repo.GetFileByChecksum(fakeFile.Sha256Checksum);
        var actual2 = await repo.GetFileByChecksum(fakeFile2.Sha256Checksum);

        // Assert
        Assert.NotNull(actual);
        Assert.NotNull(actual2);
        var expectedFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file", size: 10, isVerified: false);
        var expectedFile2 = Faker.FakeFile(id: Faker.Guid2, path: "path/to/file2.txt", checksum: "file2", size: 20,
            isVerified: false);
        Assert.True(Equal(actual, expectedFile));
        Assert.True(Equal(actual2, expectedFile2));
    }

    [Fact]
    public async Task SetAllAsUnverified_Path_OnlyRowsWithMatchingPathSetAsUnverified()
    {
        // Arrange
        var fakeFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file", size: 10, isVerified: true);
        var fakeFile2 = Faker.FakeFile(id: Faker.Guid2, path: "path/to/file2.txt", checksum: "file2", size: 20,
            isVerified: true);
        var fakeFile3 = Faker.FakeFile(id: Faker.Guid3, path: "other/path/to/file3.txt", checksum: "file3", size: 30,
            isVerified: true);

        var repo = await GetRepo();
        await repo.Add(fakeFile);
        await repo.Add(fakeFile2);
        await repo.Add(fakeFile3);

        // Act
        await repo.SetAllAsUnverified(FileSystemPath.Create("path/to"));
        var actual = await repo.GetFileByChecksum(fakeFile.Sha256Checksum);
        var actual2 = await repo.GetFileByChecksum(fakeFile2.Sha256Checksum);
        var actual3 = await repo.GetFileByChecksum(fakeFile3.Sha256Checksum);

        // Assert
        Assert.NotNull(actual);
        Assert.NotNull(actual2);
        Assert.NotNull(actual3);
        var expectedFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file", size: 10, isVerified: false);
        var expectedFile2 = Faker.FakeFile(id: Faker.Guid2, path: "path/to/file2.txt", checksum: "file2", size: 20,
            isVerified: false);
        var expectedFile3 = Faker.FakeFile(id: Faker.Guid3, path: "other/path/to/file3.txt", checksum: "file3",
            size: 30,
            isVerified: true);
        Assert.True(Equal(actual, expectedFile));
        Assert.True(Equal(actual2, expectedFile2));
        Assert.True(Equal(actual3, expectedFile3));
    }

    [Fact]
    public async Task GetUnverifiedFiles_NoParams_ReturnsAllUnverified()
    {
        // Arrange
        var fakeFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file", size: 10, isVerified: true);
        var fakeFile2 = Faker.FakeFile(id: Faker.Guid2, path: "path/to/file2.txt", checksum: "file2", size: 20,
            isVerified: false);

        var repo = await GetRepo();
        await repo.Add(fakeFile);
        await repo.Add(fakeFile2);

        // Act
        var actual = (await repo.GetUnverifiedFiles()).ToList();

        // Assert
        Assert.False(Contains(actual, fakeFile));
        Assert.True(Contains(actual, fakeFile2));
    }

    [Fact]
    public async Task GetFilesByPath_Path_ReturnsRowsWithMatchingPath()
    {
        // Arrange
        var fakeFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file", size: 10, isVerified: true);
        var fakeFile2 = Faker.FakeFile(id: Faker.Guid2, path: "path/to/file2.txt", checksum: "file2", size: 20,
            isVerified: true);
        var fakeFile3 = Faker.FakeFile(id: Faker.Guid3, path: "other/path/to/file3.txt", checksum: "file3", size: 30,
            isVerified: true);

        var repo = await GetRepo();
        await repo.Add(fakeFile);
        await repo.Add(fakeFile2);
        await repo.Add(fakeFile3);

        // Act
        var actual = (await repo.GetFilesByPath(FileSystemPath.Create("path/to"))).ToList();

        // Assert
        Assert.Equal(2, actual.Count);
        var expectedFile = Faker.FakeFile(path: "path/to/file.txt", checksum: "file", size: 10, isVerified: true);
        var expectedFile2 = Faker.FakeFile(id: Faker.Guid2, path: "path/to/file2.txt", checksum: "file2", size: 20,
            isVerified: true);
        Assert.True(Contains(actual, expectedFile));
        Assert.True(Contains(actual, expectedFile2));
    }

    private static bool Equal(DotFile d1, DotFile d2)
    {
        return d1.Id == d2.Id
               && d1.Path == d2.Path
               && d1.Sha256Checksum == d2.Sha256Checksum
               && d1.Size == d2.Size
               && d1.FileCreation == d2.FileCreation
               && d1.IsVerified == d2.IsVerified
               && d1.LastSync == d2.LastSync
               && d1.FirstSync == d2.FirstSync;
    }

    private static bool Contains(IEnumerable<DotFile> files, DotFile file)
    {
        return files.Count(x => x.Id == file.Id
                                && x.Path == file.Path
                                && x.Sha256Checksum == file.Sha256Checksum
                                && x.Size == file.Size
                                && x.FileCreation == file.FileCreation
                                && x.IsVerified == file.IsVerified
                                && x.LastSync == file.LastSync
                                && x.FirstSync == file.FirstSync) == 1;
    }

    public async Task InitializeAsync() => await _container.StartAsync();

    public async Task DisposeAsync() => await _container.StopAsync();

    private static readonly DateTimeOffset ClockTime = new(2024, 7, 26, 1, 2, 3, TimeSpan.FromHours(0));

    private async Task<NpgsqlFileRepository> GetRepo()
    {
        await using var connection = await GetDbConnection();
        await using var dbContext = GetDbContext(connection);
        await dbContext.Database.MigrateAsync();

        var stubClock = new Mock<IClock>();
        stubClock.Setup(x => x.GetUtcNow())
            .Returns(ClockTime);

        return new NpgsqlFileRepository(GetNpgsqlDataSource(), stubClock.Object);
    }

    private NpgsqlDataSource GetNpgsqlDataSource()
    {
        return new NpgsqlDataSourceBuilder(_container.GetConnectionString() + ";Pooling=false;").Build();
    }

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