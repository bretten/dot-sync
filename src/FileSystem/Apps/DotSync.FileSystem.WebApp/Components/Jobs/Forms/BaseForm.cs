using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace com.brettnamba.DotSync.FileSystem.WebApp.Components.Jobs.Forms;

public abstract class BaseForm : ComponentBase
{
    [CascadingParameter] protected IMudDialogInstance? MudDialog { get; set; }
    [Inject] protected IJobManager JobManager { get; set; } = null!;
    [Inject] protected IJobValidator JobValidator { get; set; } = null!;
    [Inject] protected ISnackbar Snackbar { get; set; } = null!;

    private bool _firstRenderDone = false;
    protected MudForm Form = null!;
    protected bool IsValid;
    protected string[] Errors = [];

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_firstRenderDone)
        {
            // This allows checking if the markup has only been rendered once
            // The param firstRender does not take into account needing to set event handler on a callback (IsSubmitButtonDisabled)
            _firstRenderDone = true;
        }

        return base.OnAfterRenderAsync(firstRender);
    }

    protected async Task Submit()
    {
        // Validate form inputs
        await Form.Validate();
        if (!Form.IsValid)
        {
            return;
        }

        // Server-side validation, validate against job manager
        var jobValidation = await ValidateJob();
        if (!jobValidation)
        {
            return;
        }

        await OnSubmit();
        MudDialog?.Close();
        var jobParameters = await GetJobParameters();
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
        Snackbar.Add($"Job added: {jobParameters.Type}", Severity.Success, options =>
        {
            options.CloseButtonClickFunc = snackbar =>
            {
                // Close it immediately, otherwise clicking the close button causes it to slowly fade
                snackbar.ForceClose();
                return Task.CompletedTask;
            };
        });
    }

    /// <summary>
    /// Returns the form inputs as <see cref="IJobParameters"/>
    /// </summary>
    protected abstract Task<IJobParameters> GetJobParameters();

    /// <summary>
    /// Will be executed on a valid form submit
    /// </summary>
    protected virtual async Task OnSubmit()
    {
        var jobParameters = await GetJobParameters();
        _ = Task.Run(() => JobManager.RunJob(jobParameters));
    }

    private async Task<bool> ValidateJob()
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

    protected bool IsSubmitButtonDisabled()
    {
        return _firstRenderDone && !IsValid;
    }
}