using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration;

/// <summary>
/// Pushes files from a source storage to a destiation
/// </summary>
public interface IFilePusher
{
    /// <summary>
    /// Pushes files with the specified prefix from the source to the destination and limits by an upload amount
    /// </summary>
    /// <param name="source">Source storage</param>
    /// <param name="pathPrefix">The prefix to filter path by</param>
    /// <param name="uploadLimitMb">The upload amount</param>
    /// <param name="destination">Destination storage</param>
    /// <returns>Pushed files</returns>
    Task<IEnumerable<DotFile>> PushFilesInStorage(StorageLocation source, string pathPrefix, long uploadLimitMb,
        StorageLocation destination);
}