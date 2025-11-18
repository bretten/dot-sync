namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

/// <summary>
/// Represents a path in a file system
/// </summary>
public readonly record struct FileSystemPath
{
    /// <summary>
    /// The underlying URI value
    /// </summary>
    private readonly Uri _uri;

    /// <summary>
    /// The path
    /// </summary>
    public string Value => _uri.OriginalString;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="uri">The underlying URI value</param>
    private FileSystemPath(Uri uri)
    {
        _uri = uri;
    }

    private FileSystemPath(string value)
    {
        _uri = new Uri(value, UriKind.Relative);
    }

    /// <summary>
    /// Creates a <see cref="FileSystemPath"/>
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns><see cref="FileSystemPath"/></returns>
    /// <exception cref="InvalidUriForFileSystemPathException">Thrown if the path could not be parsed as a valid, relative URI</exception>
    public static FileSystemPath Create(string path)
    {
        try
        {
            return new FileSystemPath(new Uri(path, UriKind.Relative));
        }
        catch (UriFormatException)
        {
            throw new InvalidUriForFileSystemPathException(
                $"A relative path is required. Could not create URI for path: {path}");
        }
    }

    /// <summary>
    /// Creates a <see cref="FileSystemPath"/>
    /// </summary>
    /// <param name="path">The path</param>
    /// <param name="replaceBackslashes">True if backslashes should be replaced with forward slashes</param>
    /// <returns><see cref="FileSystemPath"/></returns>
    public static FileSystemPath Create(string path, bool replaceBackslashes)
    {
        return Create(!replaceBackslashes ? path : path.Replace('\u005c', Path.AltDirectorySeparatorChar));
    }

    /// <summary>
    /// Thrown if the path could not be parsed as a valid, relative URI
    /// </summary>
    public sealed class InvalidUriForFileSystemPathException(string? message) : Exception(message);
}