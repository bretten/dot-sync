namespace com.brettnamba.DotSync.FileSystem.Application.Jobs;

public sealed class JobOutput : IJobOutput
{
    public IJob Job { get; }
    public FileResults FileResults { get; }

    public JobOutput(IJob job, FileResults fileResults)
    {
        Job = job;
        FileResults = fileResults;
    }
}