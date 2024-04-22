namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration;

/// <summary>
/// Defines a service that runs a verification on the specified directory
/// </summary>
public interface IDirectoryVerificationService
{
    /// <summary>
    /// Should execute a verification on the specified directory path
    /// </summary>
    /// <param name="directoryPath">The directory path to verify</param>
    /// <returns>The result of the verification</returns>
    Task<DirectoryVerificationResult> Execute(string directoryPath);
}