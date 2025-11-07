using com.brettnamba.DotSync.FileSystem.Application.Files;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using ImageMagick;
using ImageMagick.Formats;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Files;

public sealed class MagickThumbnailGenerator : IThumbnailGenerator
{
    private const string ThumbnailDir = "/thumbnails";

    public FileSystemPath DetermineThumbnailPath(DotFile file)
    {
        var dir = Directory.CreateDirectory(ThumbnailDir);
        return FileSystemPath.Create(Path.Combine(dir.FullName,
            Path.GetFileNameWithoutExtension(file.Path.Value) + ".jpg"), !OperatingSystem.IsWindows());
    }

    public async Task<Thumbnail> CreateThumbnail(DotFile file)
    {
        var extension = Path.GetExtension(file.Path.Value).ToLowerInvariant();
        if (extension == ".dng")
        {
            return await CreateThumbnailFromRaw(file);
        }

        await using var fileStream = File.Open(file.Path.Value, FileMode.Open, FileAccess.Read);
        using var image = new MagickImage(fileStream);

        var size = new MagickGeometry(image.Height / 2, image.Height / 2);

        size.IgnoreAspectRatio = false;

        image.Resize(size);
        image.Format = MagickFormat.Jpg;

        var path = DetermineThumbnailPath(file);
        await image.WriteAsync(path.Value);
        return new Thumbnail(path.Value, true);
    }

    private Task<Thumbnail> CreateThumbnailFromRaw(DotFile file)
    {
        var defines = new DngReadDefines
        {
            ReadThumbnail = true
        };

        using var image = new MagickImage();
        image.Settings.SetDefines(defines);

        // Gets the metadata of the raw
        image.Ping(file.Path.Value);
        // Get thumbnail data
        var thumbnailData = image.GetProfile("dng:thumbnail")?.ToByteArray();

        if (thumbnailData == null)
        {
            throw new UnknownRawException($"No raw thumbnail found for {file.Path.Value}");
        }

        // Read the thumbnail image
        using var thumbnail = new MagickImage(thumbnailData);
        var size = new MagickGeometry(thumbnail.Height / 2, thumbnail.Height / 2);

        size.IgnoreAspectRatio = false;

        thumbnail.Format = MagickFormat.Jpg;
        thumbnail.Resize(size);
        var path = DetermineThumbnailPath(file);
        thumbnail.Write(path.Value);
        return Task.FromResult(new Thumbnail(path.Value, true));
    }

    public sealed class UnknownRawException(string message) : Exception(message);
}