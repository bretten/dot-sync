using com.brettnamba.DotSync.Common.Application.Jobs;
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
        if (job.Parameters.ThrowException) throw new Test1JobRunnerFakeException();
        await Task.Delay(job.Parameters.Delay);
        var results = new List<string[]>()
        {
            new[] { job.Parameters.Param1, job.Parameters.Delay.ToString() }
        };
        return new JobOutput(job, new JobResults(new Dictionary<string, ResultCollection>()
            {
                { "Result", ResultCollection.Collection(results) }
            }
        ));
    }

    public sealed class Test1JobRunnerFakeException : Exception;
}

public sealed record Test1Parameters(string Param1, int Delay, bool ThrowException = false) : IJobParameters
{
    public JobType Type => JobType.Verify;

    public IReadOnlyDictionary<string, string> AsKeyValuePairs()
    {
        return new Dictionary<string, string>()
        {
            { "Param1", Param1 },
            { "Delay", Delay.ToString() },
        }.AsReadOnly();
    }
}