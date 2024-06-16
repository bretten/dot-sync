using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.TestClasses;

namespace com.brettnamba.DotSync.FileSystem.Domain.Tests.Files.TestClasses;

public static class Faker
{
    public static DotFile FakeFile(Guid? id = null, string? path = null, string? checksum = null, long size = 0,
        DateTime? fileCreation = null)
    {
        return new DotFile(id: id ?? new Guid(),
            FileSystemPath.Create(path?.AsPath() ?? string.Empty),
            FileSha256Checksum.Create(checksum ?? string.Empty),
            size,
            fileCreation ?? new DateTime(2024, 5, 25)
        );
    }

    public static StorageLocation FakeStorageLocation(StorageLocationType type = StorageLocationType.Local,
        string? path = null, long fileCount = 0, long size = 0)
    {
        return new StorageLocation(type,
            FileSystemPath.Create(path?.AsPath() ?? string.Empty),
            new StorageStatistics(fileCount, size)
        );
    }

    public static HistoricalStorageStatistics FakeHistoricalStorageStatistics(DateTimeOffset? dateTime = null,
        long fileCount = 0, long size = 0)
    {
        return new HistoricalStorageStatistics(dateTime ?? new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero),
            new StorageStatistics(fileCount: fileCount, size: size)
        );
    }
}