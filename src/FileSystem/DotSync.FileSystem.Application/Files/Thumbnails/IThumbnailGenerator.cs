using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Application.Files.Thumbnails;

public interface IThumbnailGenerator
{
    string ThumbnailContentType { get; }
    string DetermineThumbnailPath(FileSystemPath filePath);
    Task<Thumbnail> CreateThumbnail(FileSystemPath filePath);
}