using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Services;

/// <summary>
/// Represents a service that reads metadata from files
/// </summary>
public interface IFileMetadataReader
{
    /// <summary>
    /// Reads the date when a file was created
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <returns>The creation date of the file</returns>
    DateTime ReadFileCreationDate(FileSystemPath path);
}