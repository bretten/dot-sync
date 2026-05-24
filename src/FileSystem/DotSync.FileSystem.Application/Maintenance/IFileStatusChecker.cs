namespace com.brettnamba.DotSync.FileSystem.Application.Maintenance;

/// <summary>
/// Determines if files are in the system or not
/// </summary>
public interface IFileStatusChecker
{
    /// <summary>
    /// Checks if files have been added to the system at the specified path
    /// </summary>
    Task CheckFileStatus(string path);
}