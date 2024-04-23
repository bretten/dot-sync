using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.TestClasses;

namespace com.brettnamba.DotSync.FileSystem.Domain.Tests.StorageLocations.TestClasses;

public static class Faker
{
    public static StorageLocation FakeStorageLocation(string? path = null,
        StorageLocationType type = StorageLocationType.Local, long fileCount = 0, long size = 0)
    {
        return new StorageLocation(FileSystemPath.Create(path?.AsPath() ?? string.Empty),
            type,
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