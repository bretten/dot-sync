using com.brettnamba.DotSync.FileSystem.Application.Jobs.Contracts;

namespace com.brettnamba.DotSync.FileSystem.Application.Jobs.Events;

public sealed class JobFailedArgs : EventArgs
{
    public required IJob Job { get; init; }
    public required Exception Exception { get; init; }
}