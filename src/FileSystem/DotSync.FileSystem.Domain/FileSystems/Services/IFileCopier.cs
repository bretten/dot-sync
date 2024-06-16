using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;

/// <summary>
/// Defines a service that copies a file
/// </summary>
public interface IFileCopier
{
    Task CopyFile(FileSystemPath sourcePath, FileSystemPath sourceFile, FileSystemPath destination);
}