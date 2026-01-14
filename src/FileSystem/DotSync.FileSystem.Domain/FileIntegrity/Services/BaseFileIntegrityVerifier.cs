using System.Collections.Immutable;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;

/// <summary>
/// Generalized file integrity verifier
/// </summary>
/// <param name="fileRepository">Stores the expected state of the files</param>
/// <param name="checksumGenerator">Generates checksums for files</param>
public abstract class BaseFileIntegrityVerifier(
    IFileRepository fileRepository,
    IFileChecksumGenerator checksumGenerator,
    ILogger<IFileIntegrityVerifier> logger)
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
    /// Logger
    /// </summary>
    protected readonly ILogger<IFileIntegrityVerifier> Logger = logger;

    /// <summary>
    /// Verifies the integrity of all files within the specified path
    /// </summary>
    /// <param name="storageLocation">The storage location to verify</param>
    /// <param name="path">The path that will be verified</param>
    /// <param name="pathsToSkip">Paths to skip</param>
    /// <returns>Verification result for the path</returns>
    public async Task<FileSetIntegrityVerificationResult> Verify(StorageLocation storageLocation, FileSystemPath path,
        IEnumerable<FileSystemPath> pathsToSkip)
    {
        // Verify all files at the specified directory
        var results = (await VerifyDirectory(path, pathsToSkip)).ToImmutableList();

        foreach (var result in results)
        {
            if (result.Status != FileIntegrityStatus.New) continue;
            var newFile = new DotFile(Guid.NewGuid(), result.Path, result.Checksum, result.Size,
                result.FileCreated!.Value);
            await FileRepository.Add(newFile);
        }

        return new FileSetIntegrityVerificationResult(results);
    }

    /// <summary>
    /// Verifies the integrity of all files within the specified directory
    /// </summary>
    /// <param name="directoryPath">The path to the directory that will be verified</param>
    /// <param name="pathsToSkip">Paths to skip</param>
    /// <returns>Verification results for each file within the directory</returns>
    protected abstract Task<IEnumerable<FileIntegrityVerificationResult>> VerifyDirectory(FileSystemPath directoryPath,
        IEnumerable<FileSystemPath> pathsToSkip);
}