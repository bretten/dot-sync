using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Domain.Tenants;
using com.brettnamba.DotSync.Common.Extensions;
using com.brettnamba.DotSync.FileSystem.Application.Reporting;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration;

/// <summary>
/// <inheritdoc cref="IStorageLocationIntegrityVerificationService"/>
/// </summary>
public sealed class StorageLocationIntegrityVerificationService(
    IFileIntegrityVerifier fileIntegrityVerifier,
    IFileStorageRepository fileStorageRepository,
    IIntegrityReporter integrityReporter,
    IClock clock,
    ITenantContext tenantContext)
    : TenantAware(tenantContext), IStorageLocationIntegrityVerificationService
{
    /// <summary>
    /// Verifies the integrity of the files in the directory
    /// </summary>
    private readonly IFileIntegrityVerifier _fileIntegrityVerifier = fileIntegrityVerifier;

    /// <summary>
    /// Retrieves the storage location for the directory
    /// </summary>
    private readonly IFileStorageRepository _fileStorageRepository = fileStorageRepository;

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
        FileSystemPath path)
    {
        var storageLocation = await _fileStorageRepository.GetByTypeAndPath(storageLocationType, path);
        if (storageLocation == null)
        {
            throw new DirectoryNotStorageLocationException(
                $"No storage location of type {storageLocationType.GetDisplayName()} for file set {TenantContext.CurrentTenant}");
        }

        var result = await _fileIntegrityVerifier.Verify(storageLocation);

        storageLocation.UpdateStatistics(fileCount: result.FileCount, storageSize: result.TotalSize,
            _clock.GetUtcNow());
        await _fileStorageRepository.Update(storageLocation);

        var report = await _integrityReporter.OutputFileSetResult(storageLocation, result);

        return new StorageLocationIntegrityVerificationResult(storageLocation, result, report);
    }

    private sealed class DirectoryNotStorageLocationException(string? message) : Exception(message);
}