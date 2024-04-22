using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Repositories;
using DotSync.FileSystem.Application.Reporting;

namespace DotSync.FileSystem.Application.Orchestration;

/// <summary>
/// Service that runs a verification on the specified directory
/// </summary>
public sealed class DirectoryVerificationService(
    IFileIntegrityVerifier fileIntegrityVerifier,
    IStorageLocationRepository storageLocationRepository,
    IIntegrityReporter integrityReporter)
    : IDirectoryVerificationService
{
    /// <summary>
    /// Verifies the integrity of the files in the directory
    /// </summary>
    private readonly IFileIntegrityVerifier _fileIntegrityVerifier = fileIntegrityVerifier;

    /// <summary>
    /// Retrieves the storage location for the directory
    /// </summary>
    private readonly IStorageLocationRepository _storageLocationRepository = storageLocationRepository;

    /// <summary>
    /// Reports on the results of the verification
    /// </summary>
    private readonly IIntegrityReporter _integrityReporter = integrityReporter;

    /// <summary>
    /// Executes a verification on the specified directory path
    /// </summary>
    /// <param name="directoryPath">The directory path to verify</param>
    /// <returns>The result of the verification</returns>
    public async Task<DirectoryVerificationResult> Execute(string directoryPath)
    {
        var path = FileSystemPath.Create(directoryPath);

        var storageLocation = await _storageLocationRepository.GetByPath(path);
        if (storageLocation == null)
        {
            throw new DirectoryNotStorageLocationException($"No storage location for {path}");
        }

        var result = await _fileIntegrityVerifier.Verify(storageLocation.DirectoryPath);

        var report = await _integrityReporter.OutputDirectoryResult(result);

        return new DirectoryVerificationResult(result, report);
    }

    private sealed class DirectoryNotStorageLocationException(string? message) : Exception;
}