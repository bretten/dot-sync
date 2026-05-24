using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace com.brettnamba.DotSync.FileSystem.Application.Files;

/// <summary>
/// Collection of all the currently tracked files
/// </summary>
public record TrackedFiles
{
    /// <summary>
    /// Tracked file paths
    /// </summary>
    public HashSet<string> Paths { get; private set; }

    /// <summary>
    /// Mapping of tracked file checksums to paths
    /// </summary>
    public Dictionary<string, string> ChecksumsToPaths { get; private set; }

    /// <summary>
    /// Currently tracked files
    /// </summary>
    public HashSet<TrackedFile> AllFiles { get; private set; }

    public TrackedFiles(IEnumerable<DotFile> files)
    {
        var list = files.ToList();
        AllFiles = new HashSet<TrackedFile>(list.Select(x =>
            new TrackedFile(x.Path.Value, x.Sha256Checksum.Value)));
        Paths = new HashSet<string>(list.Select(x => x.Path.Value));
        ChecksumsToPaths = list.ToDictionary(x => x.Sha256Checksum.Value, x => x.Path.Value);
    }

    public void ExcludeTrackedFile(TrackedFile trackedFile)
    {
        AllFiles.Remove(trackedFile);
    }

    /// <summary>
    /// Returns true if the specified path is tracked
    /// </summary>
    public bool PathExists(string path)
    {
        return Paths.Contains(path);
    }

    /// <summary>
    /// Returns true if the specified checksum belongs to a file that is tracked
    /// </summary>
    public bool ChecksumExists(string checksum)
    {
        return ChecksumsToPaths.ContainsKey(checksum);
    }

    /// <summary>
    /// Returns true if the tracked file at the specified has the specified checksum
    /// </summary>
    public bool ChecksumMatches(string filePath, string checksum)
    {
        return ChecksumsToPaths.ContainsKey(checksum) && ChecksumsToPaths[checksum] == filePath;
    }
}