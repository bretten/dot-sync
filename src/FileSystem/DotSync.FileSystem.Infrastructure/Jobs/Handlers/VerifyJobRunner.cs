using System.Collections.Immutable;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Application.Files;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Contracts;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Execution;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Configuration;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Handlers;

public sealed class VerifyJobRunner : BaseJobRunner<VerifyParameters>
{
    private readonly IFileIntegrityVerifierFactory _verifierFactory;
    private readonly IThumbnailProvider _thumbnailProvider;

    public VerifyJobRunner(JobExecutionContext context, IClock clock, JobConfiguration jobConfiguration,
        ILogger<BaseJobRunner<VerifyParameters>> logger, IFileIntegrityVerifierFactory verifierFactory,
        IThumbnailProvider thumbnailProvider) : base(context, clock, jobConfiguration, logger)
    {
        _verifierFactory = verifierFactory;
        _thumbnailProvider = thumbnailProvider;
    }

    protected override async Task<IJobOutput> RunJob(IJob<VerifyParameters> job)
    {
        var verifier = _verifierFactory.GetBy(job.Parameters.Source);
        var result = await verifier.Verify(job.Parameters.Source,
            FileSystemPath.Create(job.Parameters.Path ?? ""),
            !string.IsNullOrWhiteSpace(job.Parameters.PathsToSkip)
                ? job.Parameters.PathsToSkip.Split(',', StringSplitOptions.TrimEntries).Select(FileSystemPath.Create)
                : new List<FileSystemPath>());

        // Generate as many thumbnails as possible. Any that fail to generate will be lazy-generated
        _ = Task.Run(async () =>
        {
            await Parallel.ForEachAsync(result.New,
                async (newFile, token) => { await _thumbnailProvider.GetThumbnail(newFile.Path); });
        });

        return new JobOutput(job, ToResults(result));
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