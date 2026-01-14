using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Execution;
using Moq;

namespace com.brettnamba.DotSync.FileSystem.Application.Tests.TestClasses;

public static class Faker
{
    public static JobExecutionContext FakeJobExecutionContext(IClock? clock = null)
    {
        return new JobExecutionContext(clock ?? Mock.Of<IClock>());
    }
}