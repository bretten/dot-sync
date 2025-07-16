using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.FileSystem;
using MetadataExtractor.Formats.QuickTime;
using Directory = MetadataExtractor.Directory;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.Services;

public sealed class CrossPlatformFileMetadataReader : IFileMetadataReader
{
    private readonly string[] _photoDateFormats =
    [
        "yyyy:MM:dd HH:mm:ss"
    ];

    private readonly string[] _videoDateFormats =
    [
        "ddd MMM dd HH:mm:ss yyyy",
        "ddd MMM dd HH:mm:ss zzz yyyy"
    ];

    public DateTime ReadFileCreationDate(FileSystemPath path)
    {
        try
        {
            IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata(path.Value);

            var exifBaseDir = directories.OfType<ExifDirectoryBase>().FirstOrDefault();
            var quicktimeMovieHeaderDir = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
            var fileDir = directories.OfType<FileMetadataDirectory>().FirstOrDefault();

            // Photo
            if (exifBaseDir != null && exifBaseDir.TryGetDateTime(ExifDirectoryBase.TagDateTime, out var exifDate))
            {
                return exifDate;
            }

            // Video
            if (quicktimeMovieHeaderDir != null &&
                quicktimeMovieHeaderDir.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagCreated, out var qtDate))
            {
                return qtDate.Kind == DateTimeKind.Local
                    ? qtDate
                    : DateTime.SpecifyKind(qtDate, DateTimeKind.Utc).ToLocalTime();
            }

            // No media date found, so use file system modified date
            if (fileDir != null && fileDir.TryGetDateTime(FileMetadataDirectory.TagFileModifiedDate, out var fileDate))
            {
                return fileDate;
            }

            // The file is a media file, but no dat ewas found
            throw new MediaHasNoDateException("No date found for the image file");

            // // Photo
            // if (exifBaseDir?.GetDescription(ExifDirectoryBase.TagDateTime) is { } dt1)
            // {
            //     return DateTime.TryParseExact(dt1, _photoDateFormats, CultureInfo.InvariantCulture,
            //         DateTimeStyles.AssumeLocal, out var date)
            //         ? date
            //         : DateTime.Parse(dt1);
            // }
            //
            // // Video
            // if (quicktimeMovieHeaderDir?.GetDescription(QuickTimeMovieHeaderDirectory.TagCreated) is { } dt2)
            // {
            //     return DateTime.TryParseExact(dt2, _videoDateFormats, CultureInfo.InvariantCulture,
            //         DateTimeStyles.AssumeLocal, out var date)
            //         ? date
            //         : DateTime.Parse(dt2);
            // }
            //
            // // No media date found, so use file system modified date
            // if (fileDir?.GetDescription(FileMetadataDirectory.TagFileModifiedDate) is { } dt3)
            // {
            //     return DateTime.Parse(dt3);
            // }
        }
        catch (ImageProcessingException)
        {
        }
        catch (FormatException)
        {
            throw;
        }

        return File.GetLastWriteTime(path.Value);
    }

    public sealed class MediaHasNoDateException(string message) : Exception(message);
}