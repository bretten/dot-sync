using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;

/// <summary>
/// Defines a service that should verify the integrity of all files within the specified directory
/// </summary>
public interface IFileIntegrityVerifier
{
    /// <summary>
    /// Should verify the integrity of all files within the specified directory
    /// </summary>
    /// <param name="directoryPath">The path to the directory that will be verified</param>
    /// <returns>Verification result for the directory</returns>
    Task<StorageLocationIntegrityVerificationResult> Verify(FileSystemPath directoryPath);
}