using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Application.Files;

public interface IThumbnailGenerator
{
    string ThumbnailContentType { get; }
    FileSystemPath DetermineThumbnailPath(FileSystemPath filePath);
    Task<Thumbnail> CreateThumbnail(FileSystemPath filePath);
}