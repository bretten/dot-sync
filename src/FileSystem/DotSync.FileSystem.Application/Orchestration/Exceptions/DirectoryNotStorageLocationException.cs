namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration.Exceptions;

public sealed class DirectoryNotStorageLocationException(string? message) : Exception(message);