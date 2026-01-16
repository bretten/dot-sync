using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects.Exceptions;

namespace com.brettnamba.DotSync.FileSystem.Domain.Tests.FileSystems.ValueObjects;

public class StoragePathTests
{
    [Theory]
    [InlineData("some/path/")]
    [InlineData("some/file_path")]
    [InlineData("some/file_path.txt")]
    // Reversed
    [InlineData(@"some\path\")]
    [InlineData(@"some\file_path")]
    [InlineData(@"some\file_path.txt")]
    public void Create_InvalidPath_ThrowsException(string path)
    {
        // Arrange
        Action action = () => StoragePath.Create(path);

        // Act
        var actual = Record.Exception(action);

        // Assert
        Assert.NotNull(actual);
        Assert.IsType<InvalidStoragePathException>(actual);
    }

    [Theory]
    [InlineData("C:/some/storage/", "C:/some/storage/")]
    [InlineData("/another/storage/", "/another/storage/")]
    // Reversed
    [InlineData(@"C:\some\storage\", "C:/some/storage/")]
    [InlineData(@"\another\storage\", "/another/storage/")]
    public void Create_Path_ReturnsStoragePath(string path, string expected)
    {
        // Act
        var actual = StoragePath.Create(path);

        // Assert
        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public void WithoutLeadingAndTrailingSlash_PathWithSlashes_ReturnsUpdatedPath()
    {
        // Arrange
        var storagePath = StoragePath.Create("/this/is/a/path/");

        // Act
        var actual = storagePath.WithoutLeadingAndTrailingSlash;

        // Assert
        Assert.Equal("this/is/a/path", actual);
    }

    [Fact]
    public void ConcatenateFilePath_FilePath_ReturnStorageAndFilePath()
    {
        // Arrange
        var filePath = FileSystemPath.Create("path/to/file.jpg");
        var storagePath = StoragePath.Create("/this/is/a/path/");

        // Act
        var actual = storagePath.ConcatenateFilePath(filePath);

        // Assert
        Assert.Equal("/this/is/a/path/path/to/file.jpg", actual);
    }
}