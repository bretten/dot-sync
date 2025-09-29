namespace com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;

/// <summary>
/// Defines a service that will generate the checksum of a file
/// </summary>
public interface IFileChecksumGenerator
{
    /// <summary>
    /// Should generate the checksum of the specified file
    /// </summary>
    /// <param name="fileInfo">The file to generate the checksum for</param>
    /// <returns>The checksum</returns>
    string GenerateChecksum(FileInfo fileInfo);
}