using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.Common.Application.Jobs.Events;
using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Infrastructure.Jobs.Execution;
using com.brettnamba.DotSync.Common.Infrastructure.Jobs.Handlers;
using com.brettnamba.DotSync.Common.Infrastructure.Jobs.Progress;
using com.brettnamba.DotSync.Common.Infrastructure.Tests.Jobs.TestClasses;
using com.brettnamba.DotSync.Common.Tests;
using Microsoft.Extensions.Logging;
using Moq;
using Faker = com.brettnamba.DotSync.Common.Application.Tests.Jobs.TestClasses.Faker;

namespace com.brettnamba.DotSync.Common.Infrastructure.Tests.Jobs.Execution;

public class HangfireJobManagerTests
{
    [Fact]
    public async Task RunJob_JobParameters_InvokesJobCompletedEvent()
    {
        /*
         * Arrange
         */
        // Mocks for the job runner
        var mockClock = Mock.Of<IClock>();
        var fakeJobContext = Faker.FakeJobExecutionContext(mockClock);
        var fakeJobConfig = TestClasses.Faker.FakeJobConfiguration();
        var mockLogger = Mock.Of<ILogger<HangfireJobManager>>();

        var progressReporter = new JobProgressReporter();

        // Job runner
        var jobRunner = new TestProgressReporterJobRunner(fakeJobContext, mockClock, fakeJobConfig,
            Mock.Of<ILogger<BaseJobRunner<TestProgressReportParameters>>>());
        var jobParams = new TestProgressReportParameters(progressReporter);

        // Dependency scope
        var (stubServiceScopeFactory, stubServiceScope) = ScopeFaker.MockScope();
        stubServiceScope.Setup(x => x.ServiceProvider.GetService(typeof(IJobRunner<TestProgressReportParameters>)))
            .Returns(jobRunner);
        stubServiceScope.Setup(x => x.ServiceProvider.GetService(typeof(JobExecutionContext)))
            .Returns(fakeJobContext);

        // Job manager
        var jobManager = new HangfireJobManager(stubServiceScopeFactory.Object, progressReporter, mockLogger);

        // EventHandlers for job manager
        jobManager.JobCreated += OnJobCreated;
        var jobCreated = false;

        jobManager.JobCompleted += OnJobCompleted;
        var jobCompleted = false;

        /*
         * Act
         */
        await jobManager.RunJob(jobParams);

        /*
         * Assert
         */
        // Job completed
        Assert.True(jobCreated);
        Assert.True(jobCompleted);
        // Job is tracked
        Assert.Single(jobManager.Jobs);
        Assert.Contains(jobManager.Jobs, x => x.Id == fakeJobContext.Id);
        // Job progress
        Assert.Contains(jobManager.GetJobLogs(fakeJobContext.Id), x => x.Equals("LogReported"));
        Assert.Equal(100, jobManager.CheckJobProgress(fakeJobContext.Id)!.AsPercent());
        Assert.NotNull(jobManager.GetJobOutput(fakeJobContext.Id));
        return;

        void OnJobCreated(object? sender, IJob e)
        {
            jobCreated = true;
        }

        void OnJobCompleted(object? sender, JobCompletedArgs e)
        {
            jobCompleted = true;
        }
    }

    [Fact]
    public async Task RunJob_FailedJob_ThrowsException()
    {
        /*
         * Arrange
         */
        // Mocks for the job runner
        var mockClock = Mock.Of<IClock>();
        var fakeJobContext = Faker.FakeJobExecutionContext(mockClock);
        var fakeJobConfig = TestClasses.Faker.FakeJobConfiguration();
        var mockLogger = Mock.Of<ILogger<HangfireJobManager>>();

        // Job runner
        var jobRunner = new Test1JobRunner(fakeJobContext, mockClock, fakeJobConfig,
            Mock.Of<ILogger<BaseJobRunner<Test1Parameters>>>());
        var jobParams = new Test1Parameters("a", 0, true);

        // Dependency scope
        var (stubServiceScopeFactory, stubServiceScope) = ScopeFaker.MockScope();
        stubServiceScope.Setup(x => x.ServiceProvider.GetService(typeof(IJobRunner<Test1Parameters>)))
            .Returns(jobRunner);
        stubServiceScope.Setup(x => x.ServiceProvider.GetService(typeof(JobExecutionContext)))
            .Returns(fakeJobContext);

        // Job manager
        var jobManager =
            new HangfireJobManager(stubServiceScopeFactory.Object, Mock.Of<IJobProgressReporter>(), mockLogger);

        // EventHandlers for job manager
        jobManager.JobFailed += OnJobFailed;
        var jobFailed = false;

        // Action since it will throw exception
        var action = async () => await jobManager.RunJob(jobParams);

        /*
         * Act
         */
        var actual = await Record.ExceptionAsync(action);

        /*
         * Assert
         */
        Assert.NotNull(actual);
        // Job completed
        Assert.True(jobFailed);
        // Job is tracked
        Assert.Single(jobManager.Jobs);
        Assert.Contains(jobManager.Jobs, x => x.Id == fakeJobContext.Id);
        return;

        void OnJobFailed(object? sender, JobFailedArgs e)
        {
            jobFailed = true;
        }
    }
}