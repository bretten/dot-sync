namespace com.brettnamba.DotSync.FileSystem.Application.Files.Thumbnails;

public sealed record ThumbnailConfiguration(string Path, int MaxWidth)
{
    public ThumbnailConfiguration() : this("/app/data/thumbnails_data", 800)
    {
    }

    public const string Section = "Thumbnails";
}