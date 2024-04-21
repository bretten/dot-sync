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
    /// Verifies the integrity of all files within the specified directory
    /// </summary>
    /// <param name="directoryPath">The path to the directory that will be verified</param>
    /// <returns>Verification result for the directory</returns>
    public async Task<StorageLocationIntegrityVerificationResult> Verify(FileSystemPath directoryPath)
    {
        var tasks = VerifyDirectory(directoryPath);
        var results = await Task.WhenAll(tasks);
        var filesNotFoundInTheDirectory = await FileRepository.GetUnverifiedFiles(directoryPath);

        return new StorageLocationIntegrityVerificationResult(results.AsReadOnly(),
            filesNotFoundInTheDirectory.ToImmutableList());
    }

    /// <summary>
    /// Verifies the integrity of all files within the specified directory
    /// </summary>
    /// <param name="directoryPath">The path to the directory that will be verified</param>
    /// <returns>Verification results for each file within the directory</returns>
    protected abstract IEnumerable<Task<FileIntegrityVerificationResult>> VerifyDirectory(FileSystemPath directoryPath);
}