using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Application.Files;

public interface IThumbnailGenerator
{
    FileSystemPath DetermineThumbnailPath(DotFile file);
    Task<Thumbnail> CreateThumbnail(DotFile file);
}