using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects.Exceptions;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

/// <summary>
/// Represents a file path
/// </summary>
public readonly record struct FileSystemPath
{
    /// <summary>
    /// The path
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="path">The underlying value</param>
    private FileSystemPath(string path)
    {
        Value = path;
    }

    /// <summary>
    /// Creates a <see cref="FileSystemPath"/>
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns><see cref="FileSystemPath"/></returns>
    /// <exception cref="InvalidFilePathException">Thrown if the path could not be parsed as a valid, relative path</exception>
    public static FileSystemPath Create(string path)
    {
        if (Path.IsPathFullyQualified(path) || Path.IsPathRooted(path))
        {
            throw new InvalidFilePathException($"File path must not specify drive: {path}");
        }

        if (path.Length > 0 && (path[0] == Path.DirectorySeparatorChar || path[0] == Path.AltDirectorySeparatorChar))
        {
            throw new InvalidFilePathException($"File path must be relative: {path}");
        }

        var normalizedPath =
            !OperatingSystem.IsWindows() ? path : path.Replace('\u005c', Path.AltDirectorySeparatorChar);
        return new FileSystemPath(normalizedPath);
    }
}