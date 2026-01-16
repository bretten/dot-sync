using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;

/// <summary>
/// Defines a service that should verify the integrity of all files within the specified path
/// </summary>
public interface IFileIntegrityVerifier
{
    /// <summary>
    /// Should verify the integrity of all files within the specified path
    /// </summary>
    /// <param name="storageLocation">The storage location to verify</param>
    /// <param name="pathPrefix">Files that have a path with this prefix will be verified</param>
    /// <param name="pathsToSkip">Paths to skip</param>
    /// <returns>Verification result for the path</returns>
    Task<FileSetIntegrityVerificationResult> Verify(StorageLocation storageLocation, string pathPrefix,
        IEnumerable<string> pathsToSkip);
}