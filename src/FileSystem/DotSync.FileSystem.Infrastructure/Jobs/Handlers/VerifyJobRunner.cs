using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Application.Files;
using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Configuration;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Handlers;

public sealed class VerifyJobRunner : BaseJobRunner<VerifyParameters>
{
    private readonly IStorageLocationIntegrityVerificationService _verifier;
    private readonly IThumbnailProvider _thumbnailProvider;

    public VerifyJobRunner(JobExecutionContext context, IClock clock, JobConfiguration jobConfiguration,
        ILogger<BaseJobRunner<VerifyParameters>> logger, IStorageLocationIntegrityVerificationService verifier,
        IThumbnailProvider thumbnailProvider) : base(context, clock, jobConfiguration, logger)
    {
        _verifier = verifier;
        _thumbnailProvider = thumbnailProvider;
    }

    protected override async Task<JobResult> RunJob(IJob<VerifyParameters> job)
    {
        var result = await _verifier.Execute(job.Parameters.StorageType,
            FileSystemPath.Create(job.Parameters.StoragePath ?? ""),
            FileSystemPath.Create(job.Parameters.VerifyPath ?? ""),
            !string.IsNullOrWhiteSpace(job.Parameters.PathsToSkip)
                ? job.Parameters.PathsToSkip.Split(',', StringSplitOptions.TrimEntries).Select(FileSystemPath.Create)
                : new List<FileSystemPath>());

        // Generate as many thumbnails as possible. Any that fail to generate will be lazy-generated
        _ = Task.Run(async () =>
        {
            await Parallel.ForEachAsync(result.Result.New,
                async (newFile, token) => { await _thumbnailProvider.GetThumbnail(newFile.Path); });
        });

        return new JobResult(job.Parameters, result);
    }
}