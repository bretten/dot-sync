using com.brettnamba.DotSync.FileSystem.Application.Jobs.Contracts;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.Files.TestClasses;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.TestClasses;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Tests.FileIntegrity.Services;

public class LocalFileSystemFileIntegrityVerifierTests
{
    private const string LocalFileSystemFilesPath =
        "FileIntegrity/Services/TestFiles/LocalFileSystemFileIntegrityVerifier/";

    [Fact]
    public async Task Verify_FileForEachCase_VerifiesValidFilesAndHandlesInvalidFiles()
    {
        /*
         * Arrange
         */
        var testPathDirectoryInfo = new DirectoryInfo(LocalFileSystemFilesPath);
        var storageLocation = Faker.FakeStorageLocation(StorageLocationType.Local, testPathDirectoryInfo.FullName);
        // This file's checksum and file path have been verified
        var verifiedFile = Faker.FakeFile(path: "verified.txt", checksum: "verified");
        // This file's checksum has been verified, but the file path has changed
        var pathChangedFile = Faker.FakeFile(path: "previous/location/path_changed.txt", checksum: "path_changed");
        // This file's checksum could not be verified, but it's path exists
        var checksumFailPathMatchFile =
            Faker.FakeFile(path: "dir/checksum_fail_path_match.txt", checksum: "checksum_fail_path_match");
        // This is a new file
        var newFile = Faker.FakeFile(path: "dir2/new_file.txt", checksum: "new_file");
        // This file is in the dir that will be skipped
        var skipFile = Faker.FakeFile(path: "skipDir/skip.txt", checksum: "skip");

        // The checksum generator should generate checksums for each file
        var stubChecksumGenerator = new Mock<IFileChecksumGenerator>();
        stubChecksumGenerator
            .Setup(x => x.GenerateChecksum(IsFileInfoWith("verified.txt")))
            .Returns(verifiedFile.Sha256Checksum.Value);
        stubChecksumGenerator
            .Setup(x => x.GenerateChecksum(IsFileInfoWith("path_changed.txt")))
            .Returns(pathChangedFile.Sha256Checksum.Value);
        stubChecksumGenerator
            .Setup(x => x.GenerateChecksum(IsFileInfoWith("checksum_fail_path_match.txt")))
            .Returns("invalid checksum");
        stubChecksumGenerator
            .Setup(x => x.GenerateChecksum(IsFileInfoWith("new_file.txt")))
            .Returns(newFile.Sha256Checksum.Value);
        // It will try to verify files by their checksum
        var stubFileRepository = new Mock<IFileRepository>();
        stubFileRepository.Setup(x => x.GetFilesByPath(FileSystemPath.Create("")))
            .ReturnsAsync(new List<DotFile>() { verifiedFile, pathChangedFile, checksumFailPathMatchFile });
        // Metadata reader
        var stubMetadataReader = new Mock<IFileMetadataReader>();
        stubMetadataReader.Setup(x => x.ReadFileCreationDate(It.IsAny<FileSystemPath>()))
            .Returns(new DateTime(2024, 4, 25));

        var verifier = new LocalFileSystemFileIntegrityVerifier(stubFileRepository.Object, stubChecksumGenerator.Object,
            Mock.Of<ILogger<IFileIntegrityVerifier>>(), stubMetadataReader.Object,
            Application.Tests.TestClasses.Faker.FakeJobExecutionContext(), Mock.Of<IJobProgressReporter>());

        /*
         * Act
         */
        var actual = await verifier.Verify(storageLocation, FileSystemPath.Create(""), new List<FileSystemPath>()
        {
            FileSystemPath.Create("skipDir")
        });

        /*
         * Assert
         */
        // There should be 4 results
        Assert.Equal(1, actual.TotalVerified);

        Assert.Single(actual.Moved);
        Assert.False(actual.Moved.Exists(x => x.Path == pathChangedFile.Path));

        Assert.Single(actual.Unverified);
        Assert.True(actual.Unverified.Exists(x => x.Path == checksumFailPathMatchFile.Path));

        Assert.Single(actual.New);
        Assert.True(actual.New.Exists(x => x.Path == newFile.Path));
    }

    private static FileInfo IsFileInfoWith(string path)
    {
        return It.Is<FileInfo>(x => x.FullName.EndsWith(path.AsPath()));
    }

    private static DotFile IsDotFileWith(string path, string checksum, bool isVerified)
    {
        return It.Is<DotFile>(x =>
            x.Path.Value == path.AsPath() && x.Sha256Checksum.Value == checksum && x.IsVerified == isVerified);
    }

    private static Func<FileIntegrityVerificationResult, bool> ResultFor(DotFile file)
    {
        return x => x.Path == file.Path && x.Checksum == file.Sha256Checksum;
    }
}