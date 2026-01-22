using System.Collections.Immutable;
using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.Common.DateAndTme;
using com.brettnamba.DotSync.Common.Infrastructure.Configuration;
using com.brettnamba.DotSync.Common.Infrastructure.Jobs.Handlers;
using com.brettnamba.DotSync.FileSystem.Domain.FileOrganization.Exceptions;
using com.brettnamba.DotSync.FileSystem.Domain.FileOrganization.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Handlers;

public sealed class SortJobRunner : BaseJobRunner<SortParameters>
{
    private readonly IStorageLocationRepository _storageLocationRepo;
    private readonly IFileSorter _fileSorter;

    public const string DefaultSortDir = "ToUpload/";

    public SortJobRunner(JobExecutionContext context, IClock clock, JobConfiguration jobConfiguration,
        ILogger<BaseJobRunner<SortParameters>> logger, IStorageLocationRepository storageLocationRepo,
        IFileSorter fileSorter) : base(context, clock, jobConfiguration, logger)
    {
        _storageLocationRepo = storageLocationRepo;
        _fileSorter = fileSorter;
    }

    protected override async Task<IJobOutput> RunJob(IJob<SortParameters> job)
    {
        var location = (await _storageLocationRepo.GetAll()).FirstOrDefault(x => x.Type == StorageLocationType.Local);
        if (location == null) throw new NoLocalStorageException("No Local storage for sorting");

        var sortSourcePath = StoragePath.Create(Path.Combine(location.Path.Value, DefaultSortDir));

        var result = await _fileSorter.Sort(sortSourcePath, location.Path);
        return new JobOutput(job, ToResults(result));
    }

    private static JobResults ToResults(IEnumerable<string> files)
    {
        var sortedFiles = files.Select(x => new[] { x }).ToImmutableList();
        return new JobResults(new Dictionary<string, ResultCollection>()
        {
            { "Sorted Files", ResultCollection.Collection(sortedFiles) }
        });
    }
}