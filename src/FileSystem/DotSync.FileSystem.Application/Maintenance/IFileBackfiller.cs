namespace com.brettnamba.DotSync.FileSystem.Application.Maintenance;

public interface IFileBackfiller
{
    Task BackfillThumbnails();
    Task BackfillSyncedFiles();
}