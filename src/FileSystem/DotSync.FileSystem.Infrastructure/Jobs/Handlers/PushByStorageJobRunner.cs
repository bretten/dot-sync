using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Configuration;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Handlers;

public sealed class PushByStorageJobRunner : BaseJobRunner<PushByStorageParameters>
{
    private readonly IFilePusher _filePusher;

    public PushByStorageJobRunner(JobExecutionContext context, IClock clock, JobConfiguration jobConfiguration,
        ILogger<BaseJobRunner<PushByStorageParameters>> logger, IFilePusher filePusher) : base(context, clock,
        jobConfiguration, logger)
    {
        _filePusher = filePusher;
    }

    protected override async Task<IJobOutput> RunJob(IJob<PushByStorageParameters> job)
    {
        var result = await _filePusher.PushFilesInStorage(job.Parameters.StorageLocationId,
            job.Parameters.PathPrefixFilter, job.Parameters.UploadLimitMb);

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