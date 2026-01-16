using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Application.Files.Thumbnails;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Contracts;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Execution;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Configuration;
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

    private static FileResults ToResult(FileSystemScannerResult result)
    {
        var newFiles = result.NewFiles.Select(x => new[] { x.Path.Value });
        return new FileResults(new Dictionary<string, IEnumerable<string[]>>()
        {
            { "New Files", newFiles }
        });
    }
}