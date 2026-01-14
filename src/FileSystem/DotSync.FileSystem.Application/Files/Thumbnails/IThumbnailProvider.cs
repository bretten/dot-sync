using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Application.Files.Thumbnails;

public interface IThumbnailProvider
{
    Task<Thumbnail> GetThumbnail(FileSystemPath filePath);
}