namespace com.brettnamba.DotSync.FileSystem.Application.Jobs;

public interface IJobOutput
{
    public IJob Job { get; }
    public FileResults FileResults { get; }
}