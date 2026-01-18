using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace com.brettnamba.DotSync.FileSystem.WebApp.Components.Jobs;

/// <summary>
/// Represents a base component for running jobs
///
/// NOTE: Originally tried baking much of the logic from the components into this class that extends ComponentBase, but it
/// requires @inherits for the Razor pages. Using @inherits doesn't seem to result in two renders (pre-render and final render)
/// </summary>
public class JobComponent : IDisposable // : ComponentBase, IDisposable, IAsyncDisposable
{
    //[Inject]
    public IDbContextFactory<FileSystemsDbContext> FileSystemsDbContextFactory { get; }

    //[Inject]
    public IJobManager JobManager { get; }

    //[Inject]
    public IJobProgressReporter ProgressReporter { get; }

    //[Inject]
    public ILogger<JobComponent> Logger { get; }

    public JobComponent(IDbContextFactory<FileSystemsDbContext> fileSystemsDbContextFactory, IJobManager jobManager,
        IJobProgressReporter progressReporter, ILogger<JobComponent> logger)
    {
        FileSystemsDbContextFactory = fileSystemsDbContextFactory;
        JobManager = jobManager;
        ProgressReporter = progressReporter;
        Logger = logger;
    }

    protected FileSystemsDbContext? StorageDbContext;
    protected IEnumerable<StorageLocation>? StorageLocations;
    protected IEnumerable<StorageLocation>? LocalStorageLocations;
    protected IEnumerable<StorageLocation>? S3StorageLocations;

    public string DefaultLocalStorage => LocalStorageLocations?.FirstOrDefault()?.Path.Value ?? "";
    public string DefaultS3Storage => S3StorageLocations?.FirstOrDefault()?.Path.Value ?? "";

    protected List<string>? Result;

    public void OnInitialized()
    {
        //JobResultProvider.JobCompleted += HandleJobResult;
        StorageDbContext = FileSystemsDbContextFactory.CreateDbContext();
        StorageLocations = StorageDbContext.StorageLocations.ToList();
        LocalStorageLocations = StorageLocations.Where(x => x.Type == StorageLocationType.Local).ToList();
        S3StorageLocations = StorageLocations.Where(x => x.Type == StorageLocationType.AmazonS3).ToList();
    }

    // protected void HandleJobResult(object? sender, JobResult data)
    // {
    //     ParseResult(sender, data);
    // }
    //
    // protected virtual void ParseResult(object? sender, JobResult data)
    // {
    // }

    public void Dispose()
    {
        StorageDbContext?.Dispose();
        //JobResultProvider.JobCompleted -= HandleJobResult;
    }
}