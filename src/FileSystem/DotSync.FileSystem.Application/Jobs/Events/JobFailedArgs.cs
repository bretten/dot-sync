namespace com.brettnamba.DotSync.FileSystem.Application.Jobs.Events;

public sealed class JobFailedArgs : EventArgs
{
    public IJob Job { get; init; } = null!;
    public Exception Exception { get; init; } = null!;
}