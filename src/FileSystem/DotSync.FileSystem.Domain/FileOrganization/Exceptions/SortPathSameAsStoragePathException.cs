namespace com.brettnamba.DotSync.FileSystem.Domain.FileOrganization.Exceptions;

public sealed class SortPathSameAsStoragePathException(string message) : Exception(message);