using com.brettnamba.DotSync.Common.Application.Jobs;
using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.Common.DateAndTme;
using Moq;

namespace com.brettnamba.DotSync.Common.Application.Tests.Jobs.TestClasses;

public static class Faker
{
    public static readonly Guid Guid1 = Guid.Parse("10000000-0000-0000-0000-000000000001");

    public static JobExecutionContext FakeJobExecutionContext(IClock? clock = null)
    {
        return new JobExecutionContext(clock ?? Mock.Of<IClock>());
    }

    public static Job<T> FakeJob<T>(T parameters, Guid? id = null, JobType? jobType = null, JobState? jobState = null)
        where T : IJobParameters
    {
        return new Job<T>(
            id: id ?? Guid1,
            type: jobType ?? JobType.Verify,
            state: jobState ?? JobState.Queued,
            parameters: parameters);
    }
}