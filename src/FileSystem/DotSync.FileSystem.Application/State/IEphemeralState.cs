namespace com.brettnamba.DotSync.FileSystem.Application.State;

/// <summary>
/// Represents ephemeral state data associated with the application
/// </summary>
public interface IEphemeralState
{
    /// <summary>
    /// Provides a collection of all directories that have files
    /// </summary>
    /// <returns><see cref="HashSet{T}"/> of all directories that have files</returns>
    Task<HashSet<string>> GetAllFileDirectories();
}