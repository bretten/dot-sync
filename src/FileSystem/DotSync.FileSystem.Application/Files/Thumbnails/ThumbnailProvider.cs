using System.Collections.Immutable;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Application.Files.Thumbnails;

public sealed class ThumbnailProvider : IThumbnailProvider
{
    private readonly IThumbnailGenerator _thumbnailGenerator;

    private static readonly IReadOnlyList<string> Extensions = new List<string>()
    {
        ".jpg", ".jpeg", ".png", ".heic", ".dng"
    }.ToImmutableList();

    public ThumbnailProvider(IThumbnailGenerator thumbnailGenerator)
    {
        _thumbnailGenerator = thumbnailGenerator;
    }

    public async Task<Thumbnail> GetThumbnail(FileSystemPath filePath)
    {
        var extension = Path.GetExtension(filePath.Value).ToLowerInvariant();
        if (!Extensions.Contains(extension))
        {
            return new Thumbnail("", _thumbnailGenerator.ThumbnailContentType);
        }

        var thumbnailPath = _thumbnailGenerator.DetermineThumbnailPath(filePath);
        if (File.Exists(thumbnailPath))
        {
            return new Thumbnail(thumbnailPath, _thumbnailGenerator.ThumbnailContentType);
        }

        return await _thumbnailGenerator.CreateThumbnail(filePath);
    }
}