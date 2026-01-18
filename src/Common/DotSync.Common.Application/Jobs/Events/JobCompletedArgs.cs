using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;

namespace com.brettnamba.DotSync.Common.Application.Jobs.Events;

public sealed class JobCompletedArgs : EventArgs
{
    public required IJob Job { get; init; }
    public required IJobOutput Result { get; init; }
}