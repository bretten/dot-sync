using System.Collections.Immutable;
using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Infrastructure.Configuration;
using com.brettnamba.DotSync.Common.Infrastructure.Jobs.Handlers;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
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
        var result = await _filePusher.PushFilesInStorage(job.Parameters.Source,
            job.Parameters.PathPrefix ?? string.Empty, job.Parameters.UploadLimitMb ?? 0, job.Parameters.Destination);

        return new JobOutput(job, ToResults(result));
    }

    private static JobResults ToResults(IEnumerable<DotFile> files)
    {
        var uploadedFiles = files.Select(x => new[] { x.Path.Value }).ToImmutableList();
        return new JobResults(new Dictionary<string, ResultCollection>()
        {
            { "Uploaded Files", ResultCollection.Collection(uploadedFiles) },
        });
    }
}