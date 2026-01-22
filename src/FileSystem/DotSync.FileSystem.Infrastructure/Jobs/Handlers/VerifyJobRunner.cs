using System.Collections.Immutable;
using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Infrastructure.Configuration;
using com.brettnamba.DotSync.Common.Infrastructure.Jobs.Handlers;
using com.brettnamba.DotSync.FileSystem.Application.Files.Thumbnails;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
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
            job.Parameters.Path ?? string.Empty,
            !string.IsNullOrWhiteSpace(job.Parameters.PathsToSkip)
                ? job.Parameters.PathsToSkip.Split(',', StringSplitOptions.TrimEntries)
                : new List<string>());

        // Generate as many thumbnails as possible. Any that fail to generate will be lazy-generated
        _ = Task.Run(async () =>
        {
            await Parallel.ForEachAsync(result.New,
                async (newFile, token) => { await _thumbnailProvider.GetThumbnail(newFile.Path); });
        });

        return new JobOutput(job, ToResults(result));
    }

    private static JobResults ToResults(FileSetIntegrityVerificationResult result)
    {
        var unverified = result.Unverified.Select(x => (string[])[x.Path.Value, x.Checksum.Value, x.Size.ToString()])
            .ToImmutableList();
        var moved = result.Moved.Select(x => (string[])[x.Path.Value]).ToImmutableList();
        var missing = result.Missing.Select(x => (string[])[x.Path.Value]).ToImmutableList();
        var newFiles = result.New.Select(x => (string[])[x.Path.Value]).ToImmutableList();
        return new JobResults(new Dictionary<string, ResultCollection>()
        {
            { "Verified", ResultCollection.Quantity(result.TotalVerified) },
            { "Unverified", ResultCollection.Collection(unverified) },
            { "Moved", ResultCollection.Collection(moved) },
            { "Missing", ResultCollection.Collection(missing) },
            { "New Files", ResultCollection.Collection(newFiles) },
        });
    }
}