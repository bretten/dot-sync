namespace com.brettnamba.DotSync.FileSystem.Application.Jobs;

public interface IJob
{
    Guid Id { get; }
    string Type { get; }
    JobState State { get; }
    DateTimeOffset StartTime { get; }
    DateTimeOffset EndTime { get; }
    TimeSpan Duration { get; }
    IJobParameters Parameters { get; }

    void SetInProgress(DateTimeOffset startTime);
    void SetDone(DateTimeOffset endTime);
    void SetError(DateTimeOffset endTime);
}

public interface IJob<out T> : IJob where T : IJobParameters
{
    new T Parameters { get; }
}