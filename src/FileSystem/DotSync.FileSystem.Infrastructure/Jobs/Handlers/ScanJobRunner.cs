using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Configuration;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Handlers;

public sealed class ScanJobRunner : BaseJobRunner<ScanParameters>
{
    private readonly IFileSystemScanner _scanner;

    public ScanJobRunner(JobExecutionContext context, IClock clock, JobConfiguration jobConfiguration,
        ILogger<BaseJobRunner<ScanParameters>> logger, IFileSystemScanner scanner) : base(context, clock,
        jobConfiguration, logger)
    {
        _scanner = scanner;
    }

    protected override async Task<JobResult> RunJob(IJob<ScanParameters> job)
    {
        var result = await _scanner.Scan(FileSystemPath.Create(job.Parameters.Path!));
        return new JobResult(job.Parameters, result);
    }
}