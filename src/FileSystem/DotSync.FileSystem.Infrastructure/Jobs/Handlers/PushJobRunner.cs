using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Configuration;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Handlers;

public sealed class PushJobRunner : BaseJobRunner<PushParameters>
{
    private readonly IFilePusher _filePusher;

    public PushJobRunner(JobExecutionContext context, IClock clock, JobConfiguration jobConfiguration,
        IJobResultProvider jobResultProvider, ILogger<BaseJobRunner<PushParameters>> logger,
        IFilePusher filePusher) : base(context, clock, jobConfiguration, jobResultProvider, logger)
    {
        _filePusher = filePusher;
    }

    protected override async Task<JobResult> RunJob(Job<PushParameters> job)
    {
        var result = await _filePusher.PushFilesInDir(StorageLocationType.Local,
            FileSystemPath.Create(job.Parameters.SourceRootPath ?? ""),
            FileSystemPath.Create(job.Parameters.SourcePushPath ?? ""),
            job.Parameters.DestinationType,
            FileSystemPath.Create(job.Parameters.DestinationPath ?? ""));

        return new JobResult(job.Parameters, result);
    }
}