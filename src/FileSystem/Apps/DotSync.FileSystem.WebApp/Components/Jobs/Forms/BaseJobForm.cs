using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.FileSystem.WebApp.Components.Widgets.Forms;
using Microsoft.AspNetCore.Components;

namespace com.brettnamba.DotSync.FileSystem.WebApp.Components.Jobs.Forms;

/// <summary>
/// Base form for creating <see cref="IJob"/>s
/// </summary>
public abstract class BaseJobForm : BaseForm
{
    [Inject] protected IJobManager JobManager { get; set; } = null!;
    [Inject] protected IJobValidator JobValidator { get; set; } = null!;

    /// <summary>
    /// Returns the form inputs as <see cref="IJobParameters"/>
    /// </summary>
    protected abstract Task<IJobParameters> GetJobParameters();

    /// <inheritdoc/>
    protected override async Task<bool> ServerSideValidate()
    {
        var jobParameters = await GetJobParameters();
        var result = await JobValidator.IsValid(jobParameters);
        if (!result.IsValid)
        {
            Errors = [result.Message!];
            return false;
        }

        Errors = [];
        return true;
    }

    /// <inheritdoc/>
    protected override async Task OnSubmit()
    {
        var jobParameters = await GetJobParameters();
        _ = Task.Run(() => JobManager.RunJob(jobParameters));
    }

    /// <inheritdoc/>
    protected override async Task<string> GetSuccessMessage()
    {
        var jobParameters = await GetJobParameters();
        return $"Job added: {jobParameters.Type}";
    }
}