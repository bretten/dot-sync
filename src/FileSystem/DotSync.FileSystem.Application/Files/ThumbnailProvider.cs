using System.Collections.Immutable;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace com.brettnamba.DotSync.FileSystem.Application.Files;

public sealed class ThumbnailProvider : IThumbnailProvider
{
    private readonly IThumbnailGenerator _thumbnailGenerator;

    private static readonly IReadOnlyList<string> _extensions = new List<string>()
    {
        ".jpg", ".jpeg", ".png", ".heic", ".dng"
    }.ToImmutableList();

    public ThumbnailProvider(IThumbnailGenerator thumbnailGenerator)
    {
        _thumbnailGenerator = thumbnailGenerator;
    }

    public async Task<Thumbnail> GetThumbnail(DotFile file)
    {
        var extension = Path.GetExtension(file.Path.Value).ToLowerInvariant();
        if (!_extensions.Contains(extension))
        {
            return new Thumbnail("", false);
        }

        var thumbnailPath = _thumbnailGenerator.DetermineThumbnailPath(file);
        if (File.Exists(thumbnailPath.Value))
        {
            return new Thumbnail(thumbnailPath.Value, true);
        }

        return await _thumbnailGenerator.CreateThumbnail(file);
    }
}