namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects.Exceptions;

/// <summary>
/// Thrown if the path is not a directory storage path
/// </summary>
public sealed class InvalidStoragePathException(string message) : Exception(message);