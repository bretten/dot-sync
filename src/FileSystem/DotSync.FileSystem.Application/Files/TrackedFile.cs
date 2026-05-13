namespace com.brettnamba.DotSync.FileSystem.Application.Files;

/// <summary>
/// Represents a file that is already tracked by the system
/// </summary>
/// <param name="Path">The file path</param>
/// <param name="Checksum">Checksum of the file</param>
public record TrackedFile(string Path, string Checksum);