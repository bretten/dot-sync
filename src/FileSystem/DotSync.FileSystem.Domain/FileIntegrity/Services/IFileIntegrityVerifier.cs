using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;

/// <summary>
/// Defines a service that should verify the integrity of all files within the specified directory
/// </summary>
public interface IFileIntegrityVerifier
{
    Task Verify(FileSystemPath directoryPath);
}