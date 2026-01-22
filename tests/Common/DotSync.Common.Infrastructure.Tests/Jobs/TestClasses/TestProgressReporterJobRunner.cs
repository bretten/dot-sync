using com.brettnamba.DotSync.Common.Application.Jobs;
using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Infrastructure.Configuration;
using com.brettnamba.DotSync.Common.Infrastructure.Jobs.Handlers;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.Common.Infrastructure.Tests.Jobs.TestClasses;

public class TestProgressReporterJobRunner : BaseJobRunner<TestProgressReportParameters>
{
    public TestProgressReporterJobRunner(JobExecutionContext context, IClock clock, JobConfiguration jobConfiguration,
        ILogger<BaseJobRunner<TestProgressReportParameters>> logger) : base(context, clock, jobConfiguration, logger)
    {
    }

    protected override Task<IJobOutput> RunJob(IJob<TestProgressReportParameters> job)
    {
        job.Parameters.ProgressReporter.ReportLog(this, job.Id, "LogReported");
        job.Parameters.ProgressReporter.ReportPercent(this, job.Id, 100, 100);
        return Task.FromResult<IJobOutput>(
            new JobOutput(job, new JobResults(new Dictionary<string, ResultCollection>()))
        );
    }
}

public sealed record TestProgressReportParameters(IJobProgressReporter ProgressReporter, bool ThrowException = false)
    : IJobParameters
{
    public JobType Type => JobType.Verify;

    public IReadOnlyDictionary<string, string> AsKeyValuePairs()
    {
        return new Dictionary<string, string>()
        {
        }.AsReadOnly();
    }
}