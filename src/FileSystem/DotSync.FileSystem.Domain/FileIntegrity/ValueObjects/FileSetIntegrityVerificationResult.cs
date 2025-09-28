using System.Collections.Immutable;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Enums;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;

/// <summary>
/// Represents the file integrity verification result of a whole file set
/// </summary>
public readonly record struct FileSetIntegrityVerificationResult
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="results">The file integrity verification results for all files within the file set</param>
    public FileSetIntegrityVerificationResult(IReadOnlyList<FileIntegrityVerificationResult> results)
    {
        // Total Verified (Count)
        TotalVerified = results.Count(x => x.Status == FileIntegrityStatus.Verified);

        // Total Unverified (Count + List)
        Unverified = results.Where(x => x.Status == FileIntegrityStatus.Unverified).ToImmutableList();

        // Total Moved (Count + List)
        Moved = results.Where(x => x.Status == FileIntegrityStatus.Moved).ToImmutableList();

        // Total Missing (Count + List)
        Missing = results.Where(x => x.Status == FileIntegrityStatus.Missing).ToImmutableList();

        // Total New (Count + List)
        New = results.Where(x => x.Status == FileIntegrityStatus.New).ToImmutableList();
    }

    public long TotalVerified { get; }
    public ImmutableList<FileIntegrityVerificationResult> Unverified { get; }
    public ImmutableList<FileIntegrityVerificationResult> Moved { get; }
    public ImmutableList<FileIntegrityVerificationResult> Missing { get; }
    public ImmutableList<FileIntegrityVerificationResult> New { get; }
}