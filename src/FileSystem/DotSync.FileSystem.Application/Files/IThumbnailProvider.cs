using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace com.brettnamba.DotSync.FileSystem.Application.Files;

public interface IThumbnailProvider
{
    Task<Thumbnail> GetThumbnail(DotFile file);
}