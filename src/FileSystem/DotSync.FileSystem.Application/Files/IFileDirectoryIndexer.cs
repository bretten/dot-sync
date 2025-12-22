namespace com.brettnamba.DotSync.FileSystem.Application.Files;

/// <summary>
/// Indexes all the directories that contains files
/// </summary>
public interface IFileDirectoryIndexer
{
    /// <summary>
    /// Returns a collection of all the directory paths that contain files
    /// </summary>
    /// <returns><see cref="HashSet{T}"/> of all directory paths that contain files</returns>
    Task<HashSet<string>> RetrieveAllFileDirectoryPaths();
}