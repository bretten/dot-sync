namespace com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;

/// <summary>
/// Represents the file integrity verification result of a whole storage location
/// </summary>
public readonly record struct StorageLocationIntegrityVerificationResult
{
    /// <summary>
    /// The file integrity verification results for all files within the storage location
    /// </summary>
    public IReadOnlyList<FileIntegrityVerificationResult> Results { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="results">The file integrity verification results for all files within the storage location</param>
    public StorageLocationIntegrityVerificationResult(IReadOnlyList<FileIntegrityVerificationResult> results)
    {
        Results = results;
        SuccessfulVerifications = Results.Count(x => x.IsVerified);
        UnverifiedFiles = Results.Where(x => !x.IsVerified);
    }

    /// <summary>
    /// The number of successful file integrity verifications
    /// </summary>
    public int SuccessfulVerifications { get; private init; }

    /// <summary>
    /// All files that were not verified
    /// </summary>
    public IEnumerable<FileIntegrityVerificationResult> UnverifiedFiles { get; private init; }
}