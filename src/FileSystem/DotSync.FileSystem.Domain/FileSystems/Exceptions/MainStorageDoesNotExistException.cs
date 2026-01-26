namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Exceptions;

/// <summary>
/// Thrown when a main storage location does not exist. The app depends on having a main storage location.
/// </summary>
public sealed class MainStorageDoesNotExistException(string message) : Exception;