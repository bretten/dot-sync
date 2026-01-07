namespace com.brettnamba.DotSync.FileSystem.Application.Jobs.Events;

public sealed class JobCompletedArgs : EventArgs
{
    public required IJob Job { get; init; }
    public required IJobOutput Result { get; init; }
}