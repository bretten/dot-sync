using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Application.Storage;

public interface IMainStorageProvider
{
    /// <summary>
    /// Returns the path to the main storage location
    /// </summary>
    /// <returns>The path to the main storage location</returns>
    Task<FileSystemPath> GetMainStoragePath();

    /// <summary>
    /// Returns the full local file path for the specified relative file path
    /// </summary>
    /// <param name="filePath">Relative file path</param>
    /// <returns>Full local file path</returns>
    Task<FileSystemPath> GetFileFullLocalPath(FileSystemPath filePath);
}