namespace com.brettnamba.DotSync.FileSystem.Application.Jobs;

/// <summary>
/// A job that represents the execution of a service
/// </summary>
/// <typeparam name="T">The job parameters type</typeparam>
public record Job<T> where T : IJobParameters
{
    public Guid Id { get; init; }
    public string Type { get; init; }
    public JobState State { get; private set; }
    public T Parameters { get; init; }

    public Job(Guid id, string type, JobState state, T parameters)
    {
        Id = id;
        Type = type;
        State = state;
        Parameters = parameters;
    }

    public void SetInProgress()
    {
        State = JobState.InProgress;
    }

    public void SetDone()
    {
        State = JobState.Done;
    }

    public void SetError()
    {
        State = JobState.Error;
    }

    public static Job<T> NewJob(Guid id, string type, T parameters)
    {
        return new Job<T>(id, type, JobState.Queued, parameters);
    }
};