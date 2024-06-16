using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.Files.TestClasses;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.TestClasses;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileStorage.Services;
using Moq;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Tests.FileStorage.Services;

public class LocalFileSystemScannerTests
{
    private const string LocalFileSystemFilesPath =
        "FileStorage/Services/TestFiles/LocalFileSystemScanner/";

    [Fact]
    public async Task Scan_DirectoryWithNewAndOldFile_ReturnsResult()
    {
        // Arrange
        var newFile = Faker.FakeFile(path: "newFile.txt", checksum: "newFile");
        var oldFile = Faker.FakeFile(path: "oldFile.txt", checksum: "oldFile");

        var stubFileRepository = new Mock<IFileStorageRepository>();
        stubFileRepository.Setup(x => x.GetFileByPath(oldFile.Path))
            .ReturnsAsync(oldFile);

        var stubFileMetadataReader = new Mock<IFileMetadataReader>();
        stubFileMetadataReader.Setup(x => x.ReadFileCreationDate(It.IsAny<FileSystemPath>()))
            .Returns(new DateTime(2024, 6, 15, 1, 2, 3));

        var stubChecksumGenerator = new Mock<IFileChecksumGenerator>();
        stubChecksumGenerator
            .Setup(x => x.GenerateChecksum(IsFileInfoWith("newFile.txt")))
            .Returns(newFile.Sha256Checksum.Value);

        var scanner = new LocalFileSystemScanner(stubFileRepository.Object, stubFileMetadataReader.Object,
            stubChecksumGenerator.Object);

        // Act
        var actual = await scanner.Scan(FileSystemPath.Create(LocalFileSystemFilesPath));

        // Assert
        Assert.Single(actual.NewFiles);
        Assert.Equal(newFile.Sha256Checksum, actual.NewFiles[0].Sha256Checksum);
        Assert.Equal(newFile.Path, actual.NewFiles[0].Path);
        Assert.Equal(new DateTime(2024, 6, 15, 1, 2, 3), actual.NewFiles[0].FileCreation);

        stubFileRepository.Verify(x =>
            x.Add(It.Is<DotFile>(y => y.Sha256Checksum == newFile.Sha256Checksum && y.Path == newFile.Path)));
    }

    private static FileInfo IsFileInfoWith(string path)
    {
        return It.Is<FileInfo>(x => x.FullName.EndsWith(path.AsPath()));
    }
}