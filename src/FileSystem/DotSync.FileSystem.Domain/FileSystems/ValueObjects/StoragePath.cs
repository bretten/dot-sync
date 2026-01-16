using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects.Exceptions;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

public readonly record struct StoragePath
{
    /// <summary>
    /// The path
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="path">The underlying value</param>
    private StoragePath(string path)
    {
        Value = path;
    }

    /// <summary>
    /// Returns the path without the leading and trailing slashes
    /// </summary>
    public string WithoutLeadingAndTrailingSlash =>
        Value.Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    /// <summary>
    /// Concatenates the storage path and file path
    /// </summary>
    /// <param name="filePath">A file path</param>
    /// <returns>This storage path concatenated with the specified file path</returns>
    public string ConcatenateFilePath(FileSystemPath filePath)
    {
        return $"{Value}{filePath.Value}";
    }

    /// <summary>
    /// Creates a <see cref="StoragePath"/>
    /// </summary>
    /// <param name="path">An absolute storage path</param>
    /// <returns><see cref="StoragePath"/></returns>
    /// <exception cref="InvalidStoragePathException">Thrown if the path is not a directory path</exception>
    public static StoragePath Create(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            throw new InvalidStoragePathException($"The path must be a rooted storage path: {path}");
        }

        if (!string.IsNullOrWhiteSpace(Path.GetFileName(path)))
        {
            throw new InvalidStoragePathException(
                $"Storage path cannot be a file path: {path} (file = {Path.GetFileName(path)}");
        }

        var normalizedPath = path.Replace('\u005c', Path.AltDirectorySeparatorChar);
        return new StoragePath(normalizedPath);
    }
}