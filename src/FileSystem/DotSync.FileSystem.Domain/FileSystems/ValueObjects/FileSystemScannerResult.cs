using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

/// <summary>
/// Represents the result of <see cref="IFileSystemScanner"/>
/// </summary>
/// <param name="NewFiles">Newly discovered files</param>
public readonly record struct FileSystemScannerResult(IReadOnlyList<DotFile> NewFiles);