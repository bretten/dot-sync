using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;

namespace com.brettnamba.DotSync.Common.Application.Jobs;

/// <summary>
/// A job that represents the execution of a service
/// </summary>
/// <typeparam name="T">The job parameters type</typeparam>
public class Job<T> : IJob<T> where T : IJobParameters
{
    public Guid Id { get; }
    public JobType Type { get; }
    public JobState State { get; private set; }
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public TimeSpan Duration => EndTime - StartTime;

    T IJob<T>.Parameters => _parameters;

    public IJobParameters Parameters => _parameters;

    private readonly T _parameters;

    public Job(Guid id, JobType type, JobState state, T parameters)
    {
        Id = id;
        Type = type;
        State = state;
        _parameters = parameters;
    }

    public void SetInProgress(DateTimeOffset startTime)
    {
        StartTime = startTime;
        State = JobState.InProgress;
    }

    public void SetDone(DateTimeOffset endTime)
    {
        EndTime = endTime;
        State = JobState.Done;
    }

    public void SetError(DateTimeOffset endTime)
    {
        EndTime = endTime;
        State = JobState.Error;
    }
};