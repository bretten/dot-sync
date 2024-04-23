using System.Collections.ObjectModel;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.TestClasses;

namespace com.brettnamba.DotSync.FileSystem.Domain.Tests.FileIntegrity.ValueObjects.TestClasses;

public static class Faker
{
    public static FileIntegrityVerificationResult FakeFileIntegrityVerificationResult(string? path = null,
        string? checksum = null, long size = 1, bool isVerified = false)
    {
        return new FileIntegrityVerificationResult(
            FileSystemPath.Create(path?.AsPath() ?? string.Empty),
            FileSha256Checksum.Create(checksum ?? string.Empty),
            size,
            isVerified
        );
    }

    public static FileSetIntegrityVerificationResult FakeFileSetIntegrityVerificationResult(
        List<FileIntegrityVerificationResult>? results = null, List<DotFile>? filesNoLongerInStorage = null)
    {
        return new FileSetIntegrityVerificationResult(
            results?.AsReadOnly() ?? ReadOnlyCollection<FileIntegrityVerificationResult>.Empty,
            filesNoLongerInStorage?.AsReadOnly() ?? ReadOnlyCollection<DotFile>.Empty);
    }
}