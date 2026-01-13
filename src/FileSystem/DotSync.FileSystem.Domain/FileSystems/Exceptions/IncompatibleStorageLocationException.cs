using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Exceptions;

/// <summary>
/// Thrown when the <see cref="StorageLocation"/> type is not compatible with the desired action
/// </summary>
/// <param name="message"></param>
public sealed class IncompatibleStorageLocationException(string message) : Exception(message);