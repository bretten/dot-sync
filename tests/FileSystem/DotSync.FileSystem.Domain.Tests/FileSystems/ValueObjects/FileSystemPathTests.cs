using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.Tests.FileSystems.ValueObjects;

public class FileSystemPathTests
{
    [Theory]
    [InlineData("file://file.jpg")]
    [InlineData("file://dir/file.png")]
    [InlineData("file://dir/dir2/test.txt")]
    public void Create_InvalidPath_ThrowsException(string path)
    {
        // Arrange
        Action action = () => FileSystemPath.Create(path);

        // Act
        var actual = Record.Exception(action);

        // Assert
        Assert.NotNull(actual);
        Assert.IsType<FileSystemPath.InvalidUriForFileSystemPathException>(actual);
    }

    [Theory]
    [InlineData("file.jpg", "file.jpg")]
    [InlineData("dir/file.png", "dir/file.png")]
    [InlineData("dir/dir2/test.txt", "dir/dir2/test.txt")]
    [InlineData(@"dir\dir2\test.txt",
        @"dir\dir2\test.txt")] // Backslashes are valid characters in filenames in Unix: https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/8.0/file-path-backslash
    [InlineData(@"C:\path\to\file.txt", @"C:\path\to\file.txt")] // Path to file
    [InlineData(@"C:\path\to\dir", @"C:\path\to\dir")]
    [InlineData(@"C:\path\to\dir\", @"C:\path\to\dir\")]
    public void Create_Path_ReturnsFileSystemPath(string path, string expected)
    {
        // Arrange

        // Act
        var actual = FileSystemPath.Create(path);

        // Assert
        Assert.Equal(expected, actual.Value);
    }
}