using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.FileIntegrity.ValueObjects.TestClasses;

namespace com.brettnamba.DotSync.FileSystem.Domain.Tests.FileIntegrity.ValueObjects;

public class StorageLocationIntegrityVerificationResultTests
{
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
        });

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
        });

        // Act
        var actual = result.UnverifiedFiles;

        // Assert
        var actualList = actual.ToList();
        Assert.Equal(2, actualList.Count);
        Assert.Equal([unverified1, unverified2], actualList);
    }
}