using System.Collections.Immutable;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Application.Files;
using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Application.Orchestration;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
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

    protected override async Task<IJobOutput> RunJob(IJob<VerifyParameters> job)
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

        return new JobOutput(job, ToResults(result.Result));
    }

    private static FileResults ToResults(FileSetIntegrityVerificationResult result)
    {
        var verified = new List<string[]>() { new string[] { "Verified Count", result.TotalVerified.ToString() } };
        var unverified = result.Unverified.Select(x => (string[])[x.Path.Value, x.Checksum.Value, x.Size.ToString()]);
        var moved = result.Moved.Select(x => (string[])[x.Path.Value]).ToImmutableList();
        var missing = result.Missing.Select(x => (string[])[x.Path.Value]).ToImmutableList();
        var newFiles = result.New.Select(x => (string[])[x.Path.Value]).ToImmutableList();
        return new FileResults(new Dictionary<string, IEnumerable<string[]>>()
        {
            { "Verified", verified },
            { "Unverified", unverified },
            { "Moved", moved },
            { "Missing", missing },
            { "New Files", newFiles },
        });
    }
}