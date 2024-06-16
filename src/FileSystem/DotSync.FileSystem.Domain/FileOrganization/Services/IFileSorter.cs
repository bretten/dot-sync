using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileOrganization.Services;

/// <summary>
/// Represents a service that sorts files from the source path to the destination path
/// </summary>
public interface IFileSorter
{
    /// <summary>
    /// Sorts files from the source path to the destination path
    /// </summary>
    /// <param name="sourcePath">The path where the files are stored</param>
    /// <param name="destinationPath">The path where the files will be sorted to</param>
    /// <returns>The completed task</returns>
    Task Sort(FileSystemPath sourcePath, FileSystemPath destinationPath);
}