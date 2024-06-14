using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.Files.TestClasses;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.TestClasses;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;
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
        // This file's checksum and file path have been verified
        var verifiedFile = Faker.FakeFile(path: "verified.txt", checksum: "verified");
        // This file's checksum has been verified, but the file path has changed
        var pathChangedFile = Faker.FakeFile(path: "previous/location/path_changed.txt", checksum: "path_changed");
        // This file's checksum could not be verified, but it's path exists
        var checksumFailPathMatchFile =
            Faker.FakeFile(path: "dir/checksum_fail_path_match.txt", checksum: "checksum_fail_path_match");
        // This is a new file
        var newFile = Faker.FakeFile(path: "dir2/new_file.txt", checksum: "new_file");

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
            .Returns(checksumFailPathMatchFile.Sha256Checksum.Value);
        stubChecksumGenerator
            .Setup(x => x.GenerateChecksum(IsFileInfoWith("new_file.txt")))
            .Returns(newFile.Sha256Checksum.Value);
        // It will try to verify files by their checksum
        var stubFileRepository = new Mock<IFileRepository>();
        stubFileRepository.Setup(x => x.GetFileByChecksum(verifiedFile.Sha256Checksum))
            .ReturnsAsync(verifiedFile);
        stubFileRepository.Setup(x => x.GetFileByChecksum(pathChangedFile.Sha256Checksum))
            .ReturnsAsync(pathChangedFile);
        stubFileRepository.Setup(x => x.GetFileByChecksum(checksumFailPathMatchFile.Sha256Checksum))
            .ReturnsAsync((DotFile?)null);
        stubFileRepository.Setup(x => x.GetFileByChecksum(newFile.Sha256Checksum))
            .ReturnsAsync((DotFile?)null);
        // It will try to verify files by their path if they could not be found by their checksum
        stubFileRepository.Setup(x => x.GetFileByPath(checksumFailPathMatchFile.Path))
            .ReturnsAsync(checksumFailPathMatchFile);
        stubFileRepository.Setup(x => x.GetFileByPath(newFile.Path))
            .ReturnsAsync((DotFile?)null);
        // Metadata reader
        var stubMetadataReader = new Mock<IFileMetadataReader>();
        stubMetadataReader.Setup(x => x.ReadFileCreationDate(It.IsAny<FileSystemPath>()))
            .Returns(new DateTime(2024, 4, 25));

        var verifier = new LocalFileSystemFileIntegrityVerifier(stubFileRepository.Object, stubChecksumGenerator.Object,
            stubMetadataReader.Object);

        /*
         * Act
         */
        var actual = await verifier.Verify(FileSystemPath.Create(LocalFileSystemFilesPath));

        /*
         * Assert
         */
        // The verified file was verified because its checksum and path matched
        Assert.True(verifiedFile.IsVerified);
        // The file that had a checksum match, but different path should be verified and have the new path
        Assert.True(pathChangedFile.IsVerified);
        Assert.Equal("dir/dir_nested/path_changed.txt".AsPath(), pathChangedFile.Path.Value);
        // The file that had no checksum match, but its path was matched should not be verified
        Assert.False(checksumFailPathMatchFile.IsVerified);
        // The new file should be added
        stubFileRepository.Verify(x => x.Add(IsDotFileWith("dir2/new_file.txt", "new_file", true)), Times.Once);
        // The other files should not have been added
        stubFileRepository.Verify(x => x.Add(It.IsAny<DotFile>()), Times.AtMostOnce);

        // There should be 4 results
        Assert.Equal(4, actual.Results.Count);
        Assert.True(actual.Results.First(ResultFor(verifiedFile)).IsVerified);
        Assert.True(actual.Results.First(ResultFor(pathChangedFile)).IsVerified);
        Assert.False(actual.Results.First(ResultFor(checksumFailPathMatchFile)).IsVerified);
        Assert.True(actual.Results.First(ResultFor(newFile)).IsVerified);
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