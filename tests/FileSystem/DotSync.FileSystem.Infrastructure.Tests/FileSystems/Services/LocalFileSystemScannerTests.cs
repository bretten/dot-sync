using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Contracts;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Execution;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.Files.TestClasses;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.TestClasses;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Tests.FileSystems.Services;

public class LocalFileSystemScannerTests
{
    private static readonly string LocalFileSystemFilesPath =
        $"{AppContext.BaseDirectory}/FileSystems/Services/TestFiles/LocalFileSystemScanner/";

    [Fact]
    public async Task Scan_DirectoryWithNewAndOldFile_ReturnsResult()
    {
        // Arrange
        var newFile = Faker.FakeFile(path: "newFile.txt", checksum: "newFile");
        var oldFile = Faker.FakeFile(path: "oldFile.txt", checksum: "oldFile");
        var nestedFile = Faker.FakeFile(path: "someDir/nestedFile.txt", checksum: "nestedFile");
        var storage = Faker.FakeStorageLocation(StorageLocationType.Local, LocalFileSystemFilesPath);

        var stubFileRepository = new Mock<IFileRepository>();
        stubFileRepository.Setup(x => x.GetFileByPath(oldFile.Path))
            .ReturnsAsync(oldFile);

        var stubFileMetadataReader = new Mock<IFileMetadataReader>();
        stubFileMetadataReader.Setup(x => x.ReadFileCreationDate(It.IsAny<string>()))
            .Returns(new DateTime(2024, 6, 15, 1, 2, 3));

        var stubChecksumGenerator = new Mock<IFileChecksumGenerator>();
        stubChecksumGenerator
            .Setup(x => x.GenerateChecksum(IsFileInfoWith("newFile.txt")))
            .Returns(newFile.Sha256Checksum.Value);
        stubChecksumGenerator
            .Setup(x => x.GenerateChecksum(IsFileInfoWith("nestedFile.txt")))
            .Returns(nestedFile.Sha256Checksum.Value);

        var scanner = new LocalFileSystemScanner(stubFileRepository.Object, stubFileMetadataReader.Object,
            stubChecksumGenerator.Object, Mock.Of<ILogger<IFileSystemScanner>>(),
            new JobExecutionContext(Mock.Of<IClock>()), Mock.Of<IJobProgressReporter>());

        // Act
        var actual = await scanner.Scan(storage);

        // Assert
        Assert.Equal(2, actual.NewFiles.Count);
        Assert.Single(actual.NewFiles, x => x.Sha256Checksum == newFile.Sha256Checksum && x.Path == newFile.Path);
        Assert.Single(actual.NewFiles, x => x.Sha256Checksum == nestedFile.Sha256Checksum && x.Path == nestedFile.Path);

        stubFileRepository.Verify(x =>
            x.Add(It.Is<DotFile>(y => y.Sha256Checksum == newFile.Sha256Checksum && y.Path == newFile.Path)));
        stubFileRepository.Verify(x =>
            x.Add(It.Is<DotFile>(y => y.Sha256Checksum == nestedFile.Sha256Checksum && y.Path == nestedFile.Path)));
    }

    [Fact]
    public async Task Scan_OnlyNestedDirectory_ReturnsResult()
    {
        // Arrange
        var oldFile = Faker.FakeFile(path: "oldFile.txt", checksum: "oldFile");
        var nestedFile = Faker.FakeFile(path: "someDir/nestedFile.txt", checksum: "nestedFile");
        var storage = Faker.FakeStorageLocation(StorageLocationType.Local, LocalFileSystemFilesPath);

        var stubFileRepository = new Mock<IFileRepository>();
        stubFileRepository.Setup(x => x.GetFileByPath(oldFile.Path))
            .ReturnsAsync(oldFile);

        var stubFileMetadataReader = new Mock<IFileMetadataReader>();
        stubFileMetadataReader.Setup(x => x.ReadFileCreationDate(It.IsAny<string>()))
            .Returns(new DateTime(2024, 6, 15, 1, 2, 3));

        var stubChecksumGenerator = new Mock<IFileChecksumGenerator>();
        stubChecksumGenerator
            .Setup(x => x.GenerateChecksum(IsFileInfoWith("nestedFile.txt")))
            .Returns(nestedFile.Sha256Checksum.Value);

        var scanner = new LocalFileSystemScanner(stubFileRepository.Object, stubFileMetadataReader.Object,
            stubChecksumGenerator.Object, Mock.Of<ILogger<IFileSystemScanner>>(),
            new JobExecutionContext(Mock.Of<IClock>()), Mock.Of<IJobProgressReporter>());

        // Act
        var actual = await scanner.Scan(storage, FileSystemPath.Create("someDir"));

        // Assert
        Assert.Equal(1, actual.NewFiles.Count);
        Assert.Single(actual.NewFiles, x => x.Sha256Checksum == nestedFile.Sha256Checksum && x.Path == nestedFile.Path);

        stubFileRepository.Verify(x =>
            x.Add(It.Is<DotFile>(y => y.Sha256Checksum == nestedFile.Sha256Checksum && y.Path == nestedFile.Path)));
    }

    private static FileInfo IsFileInfoWith(string path)
    {
        return It.Is<FileInfo>(x => x.FullName.EndsWith(path.AsPath()));
    }
}