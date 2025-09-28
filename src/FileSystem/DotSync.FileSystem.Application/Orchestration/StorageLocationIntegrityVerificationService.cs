using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Domain.Tenants;
using com.brettnamba.DotSync.Common.Extensions;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration.Exceptions;
using com.brettnamba.DotSync.FileSystem.Application.Reporting;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Repositories;

namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration;

/// <summary>
/// <inheritdoc cref="IStorageLocationIntegrityVerificationService"/>
/// </summary>
public sealed class StorageLocationIntegrityVerificationService(
    IFileIntegrityVerifierFactory fileIntegrityVerifierFactory,
    IStorageLocationRepository storageLocationRepository,
    IIntegrityReporter integrityReporter,
    IClock clock,
    ITenantContext tenantContext)
    : TenantAware(tenantContext), IStorageLocationIntegrityVerificationService
{
    /// <summary>
    /// Verifies the integrity of the files in the directory
    /// </summary>
    private readonly IFileIntegrityVerifierFactory _fileIntegrityVerifierFactory = fileIntegrityVerifierFactory;

    /// <summary>
    /// Retrieves the storage location for the directory
    /// </summary>
    private readonly IStorageLocationRepository _storageLocationRepository = storageLocationRepository;

    /// <summary>
    /// Reports on the results of the verification
    /// </summary>
    private readonly IIntegrityReporter _integrityReporter = integrityReporter;

    /// <summary>
    /// Gets the current time
    /// </summary>
    private readonly IClock _clock = clock;

    /// <summary>
    /// <inheritdoc cref="IStorageLocationIntegrityVerificationService.Execute"/>
    /// </summary>
    public async Task<StorageLocationIntegrityVerificationResult> Execute(StorageLocationType storageLocationType,
        FileSystemPath storageLocationPath, FileSystemPath verifyPath, IEnumerable<FileSystemPath> pathsToSkip)
    {
        var storageLocation =
            await _storageLocationRepository.GetByTypeAndPath(storageLocationType, storageLocationPath);
        if (storageLocation == null)
        {
            throw new DirectoryNotStorageLocationException(
                $"No storage location of type {storageLocationType.GetDisplayName()} for file set {TenantContext.CurrentTenant}");
        }

        var fileIntegrityVerifier = _fileIntegrityVerifierFactory.GetBy(storageLocation);
        var result = await fileIntegrityVerifier.Verify(verifyPath, pathsToSkip);

        await _storageLocationRepository.Update(storageLocation);

        var report = await _integrityReporter.OutputFileSetResult(storageLocation, result);

        return new StorageLocationIntegrityVerificationResult(storageLocation, result, report);
    }
}