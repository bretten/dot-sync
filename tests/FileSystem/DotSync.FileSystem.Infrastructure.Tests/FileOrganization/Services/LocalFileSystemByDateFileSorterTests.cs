using com.brettnamba.DotSync.FileSystem.Domain.FileOrganization.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileOrganization;
using Microsoft.Extensions.Logging;
using Moq;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Tests.FileOrganization.Services;

public class LocalFileSystemByDateFileSorterTests
{
    private const string SourceDirectoryName = "source/";
    private const string DestinationDirectoryName = "dest/";

    private static readonly string TestFilesPath =
        $"FileOrganization{Path.AltDirectorySeparatorChar}Services{Path.AltDirectorySeparatorChar}TestFiles";

    private readonly DirectoryInfo _testFilesDirectory = new(TestFilesPath);

    private readonly DirectoryInfo _sourceDirectory = new(SourceDirectoryName);
    private readonly DirectoryInfo _destinationDirectory = new(DestinationDirectoryName);

    [Fact]
    public async Task Sort_Files_SortsFilesByDateAndContainingDirectory()
    {
        /*
         * Arrange
         */
        // Create the source and destination directories
        CreateDirectoryFresh(_sourceDirectory);
        CreateDirectoryFresh(_destinationDirectory);
        // Copy the test files to the source directory
        CopyFiles(_testFilesDirectory, _testFilesDirectory, _sourceDirectory);

        // The metadata reader should return dates for the files
        var stubMetadataReader = new Mock<IFileMetadataReader>();
        stubMetadataReader.Setup(x => x.ReadFileCreationDate(PathEndingIn("date_modified_2024-04-25.jpg")))
            .Returns(new DateTime(2024, 4, 25));
        stubMetadataReader.Setup(x => x.ReadFileCreationDate(PathEndingIn("date_taken_2023-02-04.jpg")))
            .Returns(new DateTime(2023, 2, 4));

        var mockLogger = Mock.Of<ILogger<IFileSorter>>();
        var sorter = new LocalFileSystemByDateFileSorter(stubMetadataReader.Object,
            Mock.Of<IStorageLocationRepository>(), mockLogger);

        /*
         * Act
         */
        await sorter.Sort(StoragePath.Create(_sourceDirectory.FullName),
            StoragePath.Create(_destinationDirectory.FullName));

        /*
         * Assert
         */
        // The directories in the source directory should still be there, but should be empty
        foreach (var entry in _sourceDirectory.EnumerateFileSystemInfos())
        {
            Assert.NotNull(entry);
            Assert.IsType<DirectoryInfo>(entry);
            Assert.Empty((entry as DirectoryInfo)!.EnumerateFileSystemInfos());
        }

        // The files should now be in the destination directory
        Assert.True(File.Exists(Path.Combine(_destinationDirectory.FullName, "2024", "2024-04", "dir1 - 2024-04",
            "date_modified_2024-04-25.jpg")));
        Assert.True(File.Exists(Path.Combine(_destinationDirectory.FullName, "2023", "2023-02", "dir2 - 2023-02",
            "date_taken_2023-02-04.jpg")));

        // Cleanup
        RemoveDirectory(_sourceDirectory);
        RemoveDirectory(_destinationDirectory);
    }

    private string PathEndingIn(string fileName)
    {
        return It.Is<string>(x => x.EndsWith(fileName));
    }

    private void CreateDirectoryFresh(DirectoryInfo directory)
    {
        RemoveDirectory(directory);
        directory.Create();
    }

    private void RemoveDirectory(DirectoryInfo directory)
    {
        try
        {
            directory.Delete(true);
        }
        catch (DirectoryNotFoundException)
        {
        }
    }

    private void CopyFiles(DirectoryInfo root, DirectoryInfo source, DirectoryInfo destination)
    {
        var entries = source.EnumerateFileSystemInfos();
        foreach (var entry in entries)
        {
            switch (entry)
            {
                case FileInfo info:
                    var relativePath = Path.GetRelativePath(root.FullName, info.Directory?.FullName!);
                    var newPath = Path.Combine(destination.FullName, relativePath, info.Name);
                    new FileInfo(newPath).Directory?.Create();
                    info.CopyTo(newPath);
                    break;
                case DirectoryInfo info:
                    CopyFiles(root, info, destination);
                    break;
            }
        }
    }
}