using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Infrastructure.Tests.Jobs.TestClasses;
using Microsoft.Extensions.Logging;
using Moq;
using Faker = com.brettnamba.DotSync.Common.Application.Tests.Jobs.TestClasses.Faker;

namespace com.brettnamba.DotSync.Common.Infrastructure.Tests.Jobs.Execution;

public class HangfireJobManagerTests
{
    [Fact]
    public void RunJob_JobParameters_InvokesJobCompletedEvent()
    {
        // Arrange
        var mockClock = Mock.Of<IClock>();
        var fakeJobContext = Faker.FakeJobExecutionContext(mockClock);
        var fakeJobConfig = TestClasses.Faker.FakeJobConfiguration();
        var mockLogger = Mock.Of<ILogger<Test1JobRunner>>();
        var jobRunner = new Test1JobRunner(fakeJobContext, mockClock, fakeJobConfig, mockLogger);
        var mockServiceProvider = Mock.Of<IServiceProvider>();
    }
}