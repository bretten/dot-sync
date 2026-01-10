using com.brettnamba.DotSync.FileSystem.Application.Jobs.Execution;

namespace com.brettnamba.DotSync.FileSystem.Application.Jobs.Contracts;

public interface IJobOutput
{
    public IJob Job { get; }
    public FileResults FileResults { get; }
}