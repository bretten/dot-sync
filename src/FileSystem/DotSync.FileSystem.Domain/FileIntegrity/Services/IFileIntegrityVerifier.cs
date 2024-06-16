using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Entities;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;

/// <summary>
/// Defines a service that should verify the integrity of all files within the specified path
/// </summary>
public interface IFileIntegrityVerifier
{
    /// <summary>
    /// Should verify the integrity of all files within the specified path
    /// </summary>
    /// <param name="storageLocation">The storage location that will be verified</param>
    /// <returns>Verification result for the path</returns>
    Task<FileSetIntegrityVerificationResult> Verify(StorageLocation storageLocation);
}