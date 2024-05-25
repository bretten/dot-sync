using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
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
}