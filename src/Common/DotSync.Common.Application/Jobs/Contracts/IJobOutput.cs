using com.brettnamba.DotSync.Common.Application.Jobs.Execution;

namespace com.brettnamba.DotSync.Common.Application.Jobs.Contracts;

public interface IJobOutput
{
    public IJob Job { get; }
    public FileResults FileResults { get; }
}