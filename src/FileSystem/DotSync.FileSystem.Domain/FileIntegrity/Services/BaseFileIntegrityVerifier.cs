using System.Collections.Immutable;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;

/// <summary>
/// Generalized file integrity verifier
/// </summary>
/// <param name="fileRepository">Stores the expected state of the files</param>
/// <param name="checksumGenerator">Generates checksums for files</param>
public abstract class BaseFileIntegrityVerifier(
    IFileRepository fileRepository,
    IFileChecksumGenerator checksumGenerator)
    : IFileIntegrityVerifier
{
    /// <summary>
    /// Stores the expected state of the files
    /// </summary>
    protected readonly IFileRepository FileRepository = fileRepository;

    /// <summary>
    /// Generates checksums for files
    /// </summary>
    protected readonly IFileChecksumGenerator ChecksumGenerator = checksumGenerator;

    /// <summary>
    /// Verifies the integrity of all files within the specified path
    /// </summary>
    /// <param name="path">The path that will be verified</param>
    /// <param name="pathsToSkip">Paths to skip</param>
    /// <returns>Verification result for the path</returns>
    public async Task<FileSetIntegrityVerificationResult> Verify(FileSystemPath path, IEnumerable<FileSystemPath> pathsToSkip)
    {
        // Reset the verified flag
        await FileRepository.SetAllAsUnverified(path);

        // Verify all files at the specified directory
        var results = await VerifyDirectory(path, pathsToSkip);

        // There may have been files in the repo from a previous run, but are no longer in the current filesystem
        // Files should have been verified at this point by VerifyDirectory, so we can get the remaining unverified
        // and remove the intersection between the unverified from the recent run to determine files no longer in the filesystem
        var filesNotFoundInTheDirectory = await FileRepository.GetUnverifiedFiles();

        return new FileSetIntegrityVerificationResult(results.ToImmutableList(),
            filesNotFoundInTheDirectory.ToImmutableList());
    }

    /// <summary>
    /// Verifies the integrity of all files within the specified directory
    /// </summary>
    /// <param name="directoryPath">The path to the directory that will be verified</param>
    /// <param name="pathsToSkip">Paths to skip</param>
    /// <returns>Verification results for each file within the directory</returns>
    protected abstract Task<IEnumerable<FileIntegrityVerificationResult>> VerifyDirectory(FileSystemPath directoryPath, IEnumerable<FileSystemPath> pathsToSkip);
}