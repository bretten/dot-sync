namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Exceptions;

/// <summary>
/// Thrown when trying to add a main local storage location, but one already exists
/// </summary>
public sealed class MainStorageAlreadyExistsException(string message) : Exception(message);