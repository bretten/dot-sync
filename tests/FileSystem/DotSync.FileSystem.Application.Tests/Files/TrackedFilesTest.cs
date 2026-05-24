using com.brettnamba.DotSync.FileSystem.Application.Files;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.Files.TestClasses;

namespace com.brettnamba.DotSync.FileSystem.Application.Tests.Files;

public class TrackedFilesTest
{
    private readonly TrackedFiles _trackedFiles;

    public TrackedFilesTest()
    {
        // Arrange
        var filesInSystem = new List<DotFile>();
        var fakeFile1 = Faker.FakeFile(path: "path/to/file1.txt", checksum: "checksum1");
        var fakeFile2 = Faker.FakeFile(path: "path/to/file2.txt", checksum: "checksum2");
        filesInSystem.Add(fakeFile1);
        filesInSystem.Add(fakeFile2);

        _trackedFiles = new TrackedFiles(filesInSystem);
    }

    [Fact]
    public void PathExists_PathInTrackedFiles_ReturnsTrue()
    {
        // Act
        var actual = _trackedFiles.PathExists("path/to/file1.txt");

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void PathExists_PathNotInTrackedFiles_ReturnsFalse()
    {
        // Act
        var actual = _trackedFiles.PathExists("path/to/file3.txt");

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void ChecksumExists_ChecksumInTrackedFiles_ReturnsTrue()
    {
        // Act
        var actual = _trackedFiles.ChecksumExists("checksum1");

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void ChecksumExists_ChecksumNotInTrackedFiles_ReturnsFalse()
    {
        // Act
        var actual = _trackedFiles.ChecksumExists("checksum3");

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void ChecksumMatches_FilePathMatchesChecksum_ReturnsTrue()
    {
        // Act
        var actual = _trackedFiles.ChecksumMatches("path/to/file1.txt", "checksum1");

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void ChecksumMatches_FilePathDoesNotMatchChecksum_ReturnsFalse()
    {
        // Act
        var actual = _trackedFiles.ChecksumMatches("path/to/file2.txt", "checksum1");

        // Assert
        Assert.False(actual);
    }
}