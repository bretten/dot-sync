using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

/// <summary>
/// Represents the result of <see cref="IFileSystemScanner"/>
/// </summary>
/// <param name="NewFiles">The new files that were just synced with the domain</param>
public readonly record struct FileSystemScannerResult(
    IReadOnlyList<Tuple<FileSystemPath, FileSha256Checksum>> NewFiles);