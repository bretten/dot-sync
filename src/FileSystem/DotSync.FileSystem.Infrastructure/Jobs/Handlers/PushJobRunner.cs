using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Contracts;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Execution;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Configuration;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Handlers;

public sealed class PushJobRunner : BaseJobRunner<PushParameters>
{
    private readonly IFilePusher _filePusher;

    public PushJobRunner(JobExecutionContext context, IClock clock, JobConfiguration jobConfiguration,
        ILogger<BaseJobRunner<PushParameters>> logger, IFilePusher filePusher) : base(context, clock, jobConfiguration,
        logger)
    {
        _filePusher = filePusher;
    }

    protected override async Task<IJobOutput> RunJob(IJob<PushParameters> job)
    {
        var result = await _filePusher.PushFilesByPath(job.Parameters.Source, job.Parameters.PathPrefix ?? string.Empty,
            job.Parameters.Destination);

        return new JobOutput(job, ToResults(result));
    }

    private static FileResults ToResults(IEnumerable<DotFile> files)
    {
        var uploadedFiles = files.Select(x => new[] { x.Path.Value });
        return new FileResults(new Dictionary<string, IEnumerable<string[]>>()
        {
            { "Uploaded Files", uploadedFiles }
        });
    }
}