using com.brettnamba.DotSync.Common.DateAndTme;

namespace com.brettnamba.DotSync.FileSystem.Application.Jobs.Execution;

/// <summary>
/// Conceptually the same as <see cref="ExecutionContext"/>, but it represents the state of a single <see cref="Job"/> execution
/// </summary>
public sealed class JobExecutionContext
{
    public Guid Id { get; init; }
    public DateTimeOffset Start { get; init; }

    public JobExecutionContext(IClock clock)
    {
        Id = Guid.NewGuid();
        Start = clock.GetUtcNow();
    }
}