using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;

namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration;

/// <summary>
/// Defines a service that runs a integrity verification on the specified <see cref="StorageLocation"/>
/// </summary>
public interface IStorageLocationIntegrityVerificationService
{
    /// <summary>
    /// Should execute a integrity verification on the specified <see cref="StorageLocation"/>
    /// </summary>
    /// <param name="storageLocationType">The storage location type</param>
    /// <returns>The result of the verification</returns>
    Task<StorageLocationIntegrityVerificationResult> Execute(StorageLocationType storageLocationType);
}