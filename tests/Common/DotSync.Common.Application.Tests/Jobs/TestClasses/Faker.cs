using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.Common.DateAndTme;
using Moq;

namespace com.brettnamba.DotSync.Common.Application.Tests.Jobs.TestClasses;

public static class Faker
{
    public static JobExecutionContext FakeJobExecutionContext(IClock? clock = null)
    {
        return new JobExecutionContext(clock ?? Mock.Of<IClock>());
    }
}