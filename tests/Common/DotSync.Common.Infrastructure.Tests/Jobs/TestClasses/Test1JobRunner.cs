using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Infrastructure.Configuration;
using com.brettnamba.DotSync.Common.Infrastructure.Jobs.Handlers;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.Common.Infrastructure.Tests.Jobs.TestClasses;

public sealed class Test1JobRunner : BaseJobRunner<Test1Parameters>
{
    public Test1JobRunner(JobExecutionContext context, IClock clock, JobConfiguration jobConfiguration,
        ILogger<BaseJobRunner<Test1Parameters>> logger) : base(context, clock, jobConfiguration, logger)
    {
    }

    protected override async Task<IJobOutput> RunJob(IJob<Test1Parameters> job)
    {
        await Task.Delay(job.Parameters.Delay);
        return new JobOutput(job, new FileResults(new Dictionary<string, IEnumerable<string[]>>()));
    }
}