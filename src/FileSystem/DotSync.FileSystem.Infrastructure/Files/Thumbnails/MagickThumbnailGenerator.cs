using com.brettnamba.DotSync.FileSystem.Application.Files.Thumbnails;
using com.brettnamba.DotSync.FileSystem.Application.Storage;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using ImageMagick;
using ImageMagick.Formats;
using Microsoft.Extensions.Options;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Files.Thumbnails;

public sealed class MagickThumbnailGenerator : IThumbnailGenerator
{
    private readonly IMainStorageProvider _storageProvider;
    private readonly ThumbnailConfiguration _config;

    private DirectoryInfo? _thumbnailsDir;

    private DirectoryInfo ThumbnailsDir
    {
        get
        {
            if (_thumbnailsDir == null)
            {
                _thumbnailsDir = Directory.CreateDirectory(_config.Path);
            }

            return _thumbnailsDir;
        }
    }

    public MagickThumbnailGenerator(IMainStorageProvider storageProvider, IOptions<ThumbnailConfiguration> config)
    {
        _storageProvider = storageProvider;
        _config = config.Value;
    }

    public string ThumbnailContentType => "image/jpeg";

    public FileSystemPath DetermineThumbnailPath(FileSystemPath filePath)
    {
        var fileDirPath = Path.GetDirectoryName(filePath.Value)!;
        var thumbnailFilename = $"{Path.GetFileNameWithoutExtension(filePath.Value)}.jpg";
        return FileSystemPath.Create(Path.Combine(ThumbnailsDir.FullName, fileDirPath, thumbnailFilename),
            !OperatingSystem.IsWindows());
    }

    public async Task<Thumbnail> CreateThumbnail(FileSystemPath filePath)
    {
        var extension = Path.GetExtension(filePath.Value).ToLowerInvariant();
        if (extension == ".dng")
        {
            return await CreateThumbnailFromRaw(filePath);
        }

        var fullLocalPath = (await _storageProvider.GetFileFullLocalPath(filePath)).Value;

        await using var fileStream = File.Open(fullLocalPath, FileMode.Open, FileAccess.Read);
        using var thumbnail = new MagickImage(fileStream);

        // Resize
        ResizeThumbnail(thumbnail);

        // Compress
        Compress(thumbnail);

        // Write
        var thumbnailPath = GetThumbnailPath(filePath);
        await thumbnail.WriteAsync(thumbnailPath);

        return new Thumbnail(thumbnailPath, "image/jpeg");
    }

    private async Task<Thumbnail> CreateThumbnailFromRaw(FileSystemPath filePath)
    {
        var defines = new DngReadDefines
        {
            ReadThumbnail = true
        };

        var fullLocalPath = (await _storageProvider.GetFileFullLocalPath(filePath)).Value;

        using var raw = new MagickImage();
        raw.Settings.SetDefines(defines);

        // Gets the metadata of the raw
        raw.Ping(fullLocalPath);
        // Get thumbnail data
        var thumbnailData = raw.GetProfile("dng:thumbnail")?.ToByteArray();

        if (thumbnailData == null)
        {
            await using var fileStream = File.Open(fullLocalPath, FileMode.Open, FileAccess.Read);
            using var thumbnailRaw = new MagickImage(fileStream);
            // Resize
            ResizeThumbnail(thumbnailRaw);
            // Compress
            Compress(thumbnailRaw);
            // Write
            var path = GetThumbnailPath(filePath);
            await thumbnailRaw.WriteAsync(path);

            return new Thumbnail(path, "image/jpeg");
            //throw new UnknownRawException($"No raw thumbnail found for {fullLocalPath}");
        }

        // Read the thumbnail image
        using var thumbnail = new MagickImage(thumbnailData);

        // Resize
        ResizeThumbnail(thumbnail);

        // Compress
        Compress(thumbnail);

        // Write
        var thumbnailPath = GetThumbnailPath(filePath);
        await thumbnail.WriteAsync(thumbnailPath);

        return new Thumbnail(thumbnailPath, "image/jpeg");
    }

    private string GetThumbnailPath(FileSystemPath filePath)
    {
        var thumbnailPath = DetermineThumbnailPath(filePath);
        // Create the directory if it does not exist
        Directory.CreateDirectory(Path.GetDirectoryName(thumbnailPath.Value)!);

        return thumbnailPath.Value;
    }

    private void ResizeThumbnail(MagickImage thumbnail)
    {
        if (thumbnail.Width < _config.MaxWidth)
        {
            return;
        }

        var size = new MagickGeometry((uint)_config.MaxWidth);
        size.IgnoreAspectRatio = false;

        thumbnail.Resize(size);
    }

    private void Compress(MagickImage thumbnail)
    {
        thumbnail.SetCompression(CompressionMethod.JPEG);
        thumbnail.Format = MagickFormat.Jpg;
        thumbnail.Quality = 50;
    }

    public sealed class UnknownRawException(string message) : Exception(message);
}