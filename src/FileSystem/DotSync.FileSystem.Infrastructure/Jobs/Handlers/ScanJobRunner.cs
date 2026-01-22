using System.Collections.Immutable;
using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Infrastructure.Configuration;
using com.brettnamba.DotSync.Common.Infrastructure.Jobs.Handlers;
using com.brettnamba.DotSync.FileSystem.Application.Files.Thumbnails;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Handlers;

public sealed class ScanJobRunner : BaseJobRunner<ScanParameters>
{
    private readonly IFileSystemScanner _scanner;
    private readonly IThumbnailProvider _thumbnailProvider;

    public ScanJobRunner(JobExecutionContext context, IClock clock, JobConfiguration jobConfiguration,
        ILogger<BaseJobRunner<ScanParameters>> logger, IFileSystemScanner scanner,
        IThumbnailProvider thumbnailProvider) : base(context, clock, jobConfiguration, logger)
    {
        _scanner = scanner;
        _thumbnailProvider = thumbnailProvider;
    }

    protected override async Task<IJobOutput> RunJob(IJob<ScanParameters> job)
    {
        var result = await _scanner.Scan(job.Parameters.Source, job.Parameters.Path ?? string.Empty);

        // Generate as many thumbnails as possible. Any that fail to generate will be lazy-generated
        _ = Task.Run(async () =>
        {
            await Parallel.ForEachAsync(result.NewFiles,
                async (newFile, token) => { await _thumbnailProvider.GetThumbnail(newFile.Path); });
        });

        return new JobOutput(job, ToResult(result));
    }

    private static JobResults ToResult(FileSystemScannerResult result)
    {
        var newFiles = result.NewFiles.Select(x => new[] { x.Path.Value }).ToImmutableList();
        return new JobResults(new Dictionary<string, ResultCollection>()
        {
            { "New Files", ResultCollection.Collection(newFiles) }
        });
    }
}