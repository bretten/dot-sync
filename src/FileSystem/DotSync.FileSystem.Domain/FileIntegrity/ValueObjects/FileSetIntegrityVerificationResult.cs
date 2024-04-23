using System.Collections.Immutable;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;

/// <summary>
/// Represents the file integrity verification result of a whole file set
/// </summary>
public readonly record struct FileSetIntegrityVerificationResult
{
    /// <summary>
    /// The file integrity verification results for all files within the file set
    /// </summary>
    public IReadOnlyList<FileIntegrityVerificationResult> Results { get; }

    /// <summary>
    /// Files that have a previous record of verification but were not found during this verification of the storage
    /// location
    /// </summary>
    public IReadOnlyList<DotFile> FilesNoLongerInStorageLocation { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="results">The file integrity verification results for all files within the file set</param>
    /// <param name="filesNoLongerInStorageLocation">Files that have a previous record of verification but were not found during this verification of the file set</param>
    public FileSetIntegrityVerificationResult(IReadOnlyList<FileIntegrityVerificationResult> results,
        IReadOnlyList<DotFile> filesNoLongerInStorageLocation)
    {
        Results = results;

        FileCount = results.Count;
        TotalSize = results.Sum(x => x.Size);

        SuccessfulVerifications = Results.Count(x => x.IsVerified);
        var unverifiedFiles = Results.Where(x => !x.IsVerified).ToImmutableList();
        UnverifiedFiles = unverifiedFiles;

        // There may have been files in the repo from a previous run, but are no longer in the current filesystem
        // The results contain the verified and unverified files currently in the filesystem. By finding and removing the
        // intersection of unverified files from this current run vs the previous run, we can determine files no longer in the filesystem
        FilesNoLongerInStorageLocation = filesNoLongerInStorageLocation
            .Where(x => !unverifiedFiles.Select(u => u.Path).Contains(x.Path)).ToImmutableList();
    }

    /// <summary>
    /// The number of files
    /// </summary>
    public long FileCount { get; private init; }

    /// <summary>
    /// The total size of all the files
    /// </summary>
    public long TotalSize { get; private init; }

    /// <summary>
    /// The number of successful file integrity verifications
    /// </summary>
    public int SuccessfulVerifications { get; private init; }

    /// <summary>
    /// All files that were not verified
    /// </summary>
    public IReadOnlyList<FileIntegrityVerificationResult> UnverifiedFiles { get; private init; }
}