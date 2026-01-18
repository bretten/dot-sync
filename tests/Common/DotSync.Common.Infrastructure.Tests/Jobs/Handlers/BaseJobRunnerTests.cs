using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Infrastructure.Tests.Jobs.TestClasses;
using Microsoft.Extensions.Logging;
using Moq;
using Faker = com.brettnamba.DotSync.Common.Application.Tests.Jobs.TestClasses.Faker;

namespace com.brettnamba.DotSync.Common.Infrastructure.Tests.Jobs.Handlers;

public class BaseJobRunnerTests
{
    [Fact]
    public async Task Execute_ValidJob_ReturnsJobOutput()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 01, 18, 1, 1, 1, TimeSpan.Zero);
        var now2 = new DateTimeOffset(2026, 01, 18, 1, 1, 2, TimeSpan.Zero);
        var mockClock = new Mock<IClock>();
        mockClock.SetupSequence(x => x.GetUtcNow()).Returns(now).Returns(now2);

        var fakeJobContext = Faker.FakeJobExecutionContext();
        var fakeJobConfig = TestClasses.Faker.FakeJobConfiguration();
        var mockLogger = Mock.Of<ILogger<Test1JobRunner>>();
        var jobRunner = new Test1JobRunner(fakeJobContext, mockClock.Object, fakeJobConfig, mockLogger);
        var jobParams = new Test1Parameters("a", Delay: 0);
        var job = Faker.FakeJob(parameters: jobParams);

        // Act
        var actual = await jobRunner.Execute(job);

        // Assert
        Assert.Equal(now, job.StartTime);
        Assert.Equal(now2, job.EndTime);
        Assert.Equal(jobParams.Param1, actual.FileResults.FileResultLists["Result"].First()[0]);
        Assert.Equal(jobParams.Delay.ToString(), actual.FileResults.FileResultLists["Result"].First()[1]);
    }

    [Fact]
    public async Task Execute_ErrorJob_ThrowsException()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 01, 18, 1, 1, 1, TimeSpan.Zero);
        var now2 = new DateTimeOffset(2026, 01, 18, 1, 1, 2, TimeSpan.Zero);
        var mockClock = new Mock<IClock>();
        mockClock.SetupSequence(x => x.GetUtcNow()).Returns(now).Returns(now2);

        var fakeJobContext = Faker.FakeJobExecutionContext();
        var fakeJobConfig = TestClasses.Faker.FakeJobConfiguration();
        var mockLogger = Mock.Of<ILogger<Test1JobRunner>>();
        var jobRunner = new Test1JobRunner(fakeJobContext, mockClock.Object, fakeJobConfig, mockLogger);
        var jobParams = new Test1Parameters("a", Delay: 0, true);
        var job = Faker.FakeJob(parameters: jobParams);

        var action = async () => await jobRunner.Execute(job);

        // Act
        var actual = await Record.ExceptionAsync(action);

        // Assert
        Assert.IsType<Test1JobRunner.Test1JobRunnerFakeException>(actual);
        Assert.Equal(now, job.StartTime);
        Assert.Equal(now2, job.EndTime);
    }
}