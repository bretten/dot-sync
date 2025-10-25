namespace com.brettnamba.DotSync.FileSystem.Domain.FileOrganization.Exceptions;

public sealed class NoLocalStorageException(string message) : Exception(message);