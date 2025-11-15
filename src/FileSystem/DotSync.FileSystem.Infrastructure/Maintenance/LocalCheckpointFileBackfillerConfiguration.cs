namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Maintenance;

public sealed record LocalCheckpointFileBackfillerConfiguration(string CheckpointPath, int BatchCount)
{
    public LocalCheckpointFileBackfillerConfiguration() : this("/app/data/maintenance_data", 500)
    {
    }

    public const string Section = "Maintenance:LocalCheckpointFileBackfiller";
}