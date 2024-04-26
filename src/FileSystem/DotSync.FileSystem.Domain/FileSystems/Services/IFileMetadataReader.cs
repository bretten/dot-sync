using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;

/// <summary>
/// Represents a service that reads metadata from files
/// </summary>
public interface IFileMetadataReader
{
    /// <summary>
    /// Reads the date when a photo or video was taken
    /// </summary>
    /// <param name="path">The path of the photo or video file</param>
    /// <returns>The date the photo or video was taken</returns>
    DateTime ReadPhotoOrVideoTakenDate(FileSystemPath path);
}