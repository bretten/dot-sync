using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Application.Storage;

public interface IMainStorageProvider
{
    /// <summary>
    /// Returns the path to the main storage location
    /// </summary>
    /// <returns>The path to the main storage location</returns>
    Task<FileSystemPath> GetMainStoragePath();
}