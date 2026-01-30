using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using MudBlazor;

namespace com.brettnamba.DotSync.Apps.Common.Components.Jobs;

/// <summary>
/// Provides a way to get details about a <see cref="IJob"/>
/// </summary>
public class JobDetailsProvider : IDisposable
{
    private readonly IJobManager _jobManager;
    private readonly IDialogService _dialogService;

    public JobDetailsProvider(IJobManager jobManager, IDialogService dialogService)
    {
        _jobManager = jobManager;
        _dialogService = dialogService;

        _jobManager.JobCreated += HandleNewJob;
    }

    /// <summary>
    /// Displays a job details dialog
    /// </summary>
    /// <param name="job">The <see cref="IJob"/> to display</param>
    public Task HandleDialogOpen(IJob job)
    {
        var parameters = new DialogParameters<JobDetailsDialog>
        {
            { x => x.Job, job }
        };
        return _dialogService.ShowAsync<JobDetailsDialog>(job.Type.ToString(), parameters,
            new DialogOptions()
            {
                Position = DialogPosition.TopCenter,
                FullWidth = true,
                MaxWidth = MaxWidth.Medium,
                CloseButton = true
            });
    }

    /// <summary>
    /// Launches the job dialog when a new job is started
    /// </summary>
    private void HandleNewJob(object? sender, IJob job)
    {
        HandleDialogOpen(job);
    }

    public void Dispose()
    {
        _jobManager.JobCreated -= HandleNewJob;
    }
}