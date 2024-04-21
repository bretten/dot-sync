using System.Collections.Immutable;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.FileIntegrity.ValueObjects.TestClasses;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.TestClasses;

namespace com.brettnamba.DotSync.FileSystem.Domain.Tests.FileIntegrity.ValueObjects;

public class StorageLocationIntegrityVerificationResultTests
{
    [Fact]
    public void FileCount_ResultsAndFilesFromPreviousRun_ReturnsCountBasedOnOnlyResults()
    {
        // Arrange
        var verified = Faker.FakeFileIntegrityVerificationResult(path: "dir/file1.txt", isVerified: false);
        var unverified = Faker.FakeFileIntegrityVerificationResult(path: "dir/file2.txt", isVerified: false);

        var unverifiedFilesFromPreviousRun = new List<DotFile>()
        {
            Files.TestClasses.Faker.FakeFile(path: "dir/file1.txt"),
            Files.TestClasses.Faker.FakeFile(path: "dir/file2.txt"),
            Files.TestClasses.Faker.FakeFile(path: "dir/file3.txt"),
            Files.TestClasses.Faker.FakeFile(path: "dir/file4.txt")
        }.AsReadOnly();

        var result = new StorageLocationIntegrityVerificationResult(new List<FileIntegrityVerificationResult>()
        {
            verified, unverified
        }, unverifiedFilesFromPreviousRun);

        // Act
        var actual = result.FileCount;

        // Assert
        Assert.Equal(2, actual);
    }

    [Fact]
    public void TotalSize_ResultsAndFilesFromPreviousRun_ReturnsSizeBasedOnOnlyResults()
    {
        // Arrange
        var verified = Faker.FakeFileIntegrityVerificationResult(path: "dir/file1.txt", isVerified: false, size: 1);
        var unverified = Faker.FakeFileIntegrityVerificationResult(path: "dir/file2.txt", isVerified: false, size: 2);

        var unverifiedFilesFromPreviousRun = new List<DotFile>()
        {
            Files.TestClasses.Faker.FakeFile(path: "dir/file1.txt"),
            Files.TestClasses.Faker.FakeFile(path: "dir/file2.txt"),
            Files.TestClasses.Faker.FakeFile(path: "dir/file3.txt"),
            Files.TestClasses.Faker.FakeFile(path: "dir/file4.txt")
        }.AsReadOnly();

        var result = new StorageLocationIntegrityVerificationResult(new List<FileIntegrityVerificationResult>()
        {
            verified, unverified
        }, unverifiedFilesFromPreviousRun);

        // Act
        var actual = result.TotalSize;

        // Assert
        Assert.Equal(3, actual);
    }

    [Fact]
    public void SuccessfulVerifications_ResultsWithVerifications_ReturnsVerifiedCount()
    {
        // Arrange
        var verified1 = Faker.FakeFileIntegrityVerificationResult(isVerified: true);
        var verified2 = Faker.FakeFileIntegrityVerificationResult(isVerified: true);
        var unverified1 = Faker.FakeFileIntegrityVerificationResult(isVerified: false);
        var result = new StorageLocationIntegrityVerificationResult(new List<FileIntegrityVerificationResult>()
        {
            verified1, verified2, unverified1
        }, ImmutableList<DotFile>.Empty);

        // Act
        var actual = result.SuccessfulVerifications;

        // Assert
        Assert.Equal(2, actual);
    }

    [Fact]
    public void UnverifiedFiles_ResultsWithUnverifiedFiles_ReturnsUnverifiedFiles()
    {
        // Arrange
        var verified1 = Faker.FakeFileIntegrityVerificationResult(isVerified: true);
        var unverified1 = Faker.FakeFileIntegrityVerificationResult(isVerified: false);
        var unverified2 = Faker.FakeFileIntegrityVerificationResult(isVerified: false);
        var result = new StorageLocationIntegrityVerificationResult(new List<FileIntegrityVerificationResult>()
        {
            verified1, unverified1, unverified2
        }, ImmutableList<DotFile>.Empty);

        // Act
        var actual = result.UnverifiedFiles;

        // Assert
        Assert.Equal(2, actual.Count);
        Assert.Equal([unverified1, unverified2], actual);
    }

    [Fact]
    public void FilesNoLongerInStorageLocation_ResultsWithUnverifiedFiles_ReturnsFilesExcludingUnverifiedFileResults()
    {
        // Arrange
        var unverified1 = Faker.FakeFileIntegrityVerificationResult(path: "dir/file1.txt", isVerified: false);
        var unverified2 = Faker.FakeFileIntegrityVerificationResult(path: "dir/file2.txt", isVerified: false);

        var unverifiedFilesFromPreviousRun = new List<DotFile>()
        {
            Files.TestClasses.Faker.FakeFile(path: "dir/file1.txt"),
            Files.TestClasses.Faker.FakeFile(path: "dir/file2.txt"),
            Files.TestClasses.Faker.FakeFile(path: "dir/file3.txt"),
            Files.TestClasses.Faker.FakeFile(path: "dir/file4.txt")
        }.AsReadOnly();

        var result = new StorageLocationIntegrityVerificationResult(new List<FileIntegrityVerificationResult>()
        {
            unverified1, unverified2
        }, unverifiedFilesFromPreviousRun);

        // Act
        var actual = result.FilesNoLongerInStorageLocation;

        // Assert
        Assert.Equal(2, actual.Count);
        Assert.Equal(1, actual.Count(x => x.Path.Value == "dir/file3.txt".AsPath()));
        Assert.Equal(1, actual.Count(x => x.Path.Value == "dir/file4.txt".AsPath()));
    }
}