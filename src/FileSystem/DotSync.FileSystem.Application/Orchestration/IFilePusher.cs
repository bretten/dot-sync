using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration;

/// <summary>
/// Pushes files from a source storage to a destiation
/// </summary>
public interface IFilePusher
{
    /// <summary>
    /// Pushes files from the source path to destination
    /// </summary>
    /// <param name="source">Source storage</param>
    /// <param name="sourcePath">The path on the source to push</param>
    /// <param name="destination">Destination storage</param>
    /// <returns>Pushed files</returns>
    Task<IEnumerable<DotFile>> PushFilesByPath(StorageLocation source, FileSystemPath sourcePath,
        StorageLocation destination);

    /// <summary>
    /// Pushes files with the specified prefix from the source to the destination and limits by an upload amount
    /// </summary>
    /// <param name="source">Source storage</param>
    /// <param name="prefixFilter">The prefix to filter by</param>
    /// <param name="uploadLimitMb">The upload amount</param>
    /// <param name="destination">Destination storage</param>
    /// <returns>Pushed files</returns>
    Task<IEnumerable<DotFile>> PushFilesInStorage(StorageLocation source, string prefixFilter, long uploadLimitMb,
        StorageLocation destination);
}