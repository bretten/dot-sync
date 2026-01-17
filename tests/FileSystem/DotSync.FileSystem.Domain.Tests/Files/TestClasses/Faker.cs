using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.TestClasses;

namespace com.brettnamba.DotSync.FileSystem.Domain.Tests.Files.TestClasses;

public static class Faker
{
    public static readonly Guid Guid1 = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid Guid2 = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid Guid3 = Guid.Parse("10000000-0000-0000-0000-000000000003");
    public static readonly Guid Guid4 = Guid.Parse("10000000-0000-0000-0000-000000000004");

    public static DotFile FakeFile(Guid? id = null, string? path = null, string? checksum = null, long size = 0,
        DateTime? fileCreation = null, bool? isVerified = false, DateTimeOffset? lastSync = null,
        DateTimeOffset? firstSync = null)
    {
        var f = new DotFile(id: id ?? Guid1,
            FileSystemPath.Create(path?.AsPath() ?? string.Empty),
            FileSha256Checksum.Create(checksum ?? string.Empty),
            size,
            fileCreation ?? new DateTime(2024, 5, 25))
        {
            LastSync = lastSync ?? new DateTimeOffset(2024, 7, 26, 1, 2, 3, TimeSpan.FromHours(0)),
            FirstSync = firstSync ?? new DateTimeOffset(2024, 7, 26, 4, 5, 6, TimeSpan.FromHours(0))
        };
        if (isVerified.HasValue && isVerified.Value) f.SetAsVerified();
        return f;
    }

    public static StorageLocation FakeStorageLocation(StorageLocationType type = StorageLocationType.Local,
        string? path = null, long fileCount = 0, long size = 0)
    {
        return new StorageLocation(type, StoragePath.Create(path?.AsPath() ?? "/some/path/"));
    }
}