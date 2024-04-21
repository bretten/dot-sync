using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.TestClasses;

namespace com.brettnamba.DotSync.FileSystem.Domain.Tests.FileIntegrity.ValueObjects.TestClasses;

public static class Faker
{
    public static FileIntegrityVerificationResult FakeFileIntegrityVerificationResult(string? path = null,
        string? checksum = null, bool isVerified = false)
    {
        return new FileIntegrityVerificationResult(
            FileSystemPath.Create(path?.AsPath() ?? string.Empty),
            FileSha256Checksum.Create(checksum ?? string.Empty),
            isVerified
        );
    }
}