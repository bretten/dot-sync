namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects.Exceptions;

/// <summary>
/// Thrown if the path could not be parsed as a valid, relative file path
/// </summary>
public sealed class InvalidFilePathException(string? message) : Exception(message);