using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Services;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileStorage.ValueObjects;

/// <summary>
/// Represents the result of <see cref="IFileSystemScanner"/>
/// </summary>
/// <param name="NewFiles">The new files that were just synced with the domain</param>
public readonly record struct FileSystemScannerResult(IReadOnlyList<DotFile> NewFiles);