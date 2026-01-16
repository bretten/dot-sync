using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects.Exceptions;

namespace com.brettnamba.DotSync.FileSystem.Domain.Tests.FileSystems.ValueObjects;

public class FileSystemPathTests
{
    [Theory]
    [InlineData("C:/dir/file.jpg")]
    [InlineData("C:dir/file.jpg")]
    [InlineData("/some/storage/file.png")]
    // Reversed
    [InlineData(@"C:\dir\file.jpg")]
    [InlineData(@"C:dir\file.jpg")]
    [InlineData(@"\some\storage\file.png")]
    public void Create_InvalidPath_ThrowsException(string path)
    {
        // Arrange
        Action action = () => FileSystemPath.Create(path);

        // Act
        var actual = Record.Exception(action);

        // Assert
        Assert.NotNull(actual);
        Assert.IsType<InvalidFilePathException>(actual);
    }

    // Backslashes are valid characters in filenames in Unix: https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/8.0/file-path-backslash
    [Theory]
    [InlineData("file.jpg", "file.jpg", "file.jpg")]
    [InlineData("dir/file.png", "dir/file.png", "dir/file.png")]
    [InlineData("dir/dir2/test.txt", "dir/dir2/test.txt", "dir/dir2/test.txt")]
    // Reversed
    [InlineData(@"dir\file.png", @"dir\file.png", "dir/file.png")]
    [InlineData(@"dir\dir2\test.txt", @"dir\dir2\test.txt", "dir/dir2/test.txt")]
    public void Create_Path_ReturnsFileSystemPath(string path, string expected, string expectedWindows)
    {
        // Act
        var actual = FileSystemPath.Create(path);

        // Assert
        Assert.Equal(OperatingSystem.IsWindows() ? expectedWindows : expected, actual.Value);
    }
}