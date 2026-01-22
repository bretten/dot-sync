using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace com.brettnamba.DotSync.FileSystem.WebApp.Components.Widgets.Forms;

/// <summary>
/// Base form
/// </summary>
public abstract class BaseForm : ComponentBase
{
    /// <summary>
    /// Optional dialog that the form can appear in
    /// </summary>
    [CascadingParameter]
    protected IMudDialogInstance? MudDialog { get; set; }

    /// <summary>
    /// Used for notifications after the form is submitted
    /// </summary>
    [Inject]
    protected ISnackbar Snackbar { get; set; } = null!;

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

    /// <summary>
    /// Validates against the server, or in the case of server interactive mode, against injected components
    /// </summary>
    /// <returns></returns>
    protected abstract Task<bool> ServerSideValidate();

    /// <summary>
    /// Will be executed on a valid form submit
    /// </summary>
    protected abstract Task OnSubmit();

    /// <summary>
    /// Returns the message for a successful form submit
    /// </summary>
    protected abstract Task<string> GetSuccessMessage();

    /// <summary>
    /// Submits the form
    /// </summary>
    protected virtual async Task Submit()
    {
        // Validate form inputs client-side
        await Form.Validate();
        if (!Form.IsValid)
        {
            return;
        }

        // Server-side validation
        var serverSideValid = await ServerSideValidate();
        if (!serverSideValid)
        {
            return;
        }

        // Submit the form
        await OnSubmit();
        MudDialog?.Close(); // Close if there was an open dialog

        // Notify the user of the successful submit
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
        Snackbar.Add(await GetSuccessMessage(), Severity.Success, options =>
        {
            options.CloseButtonClickFunc = snackbar =>
            {
                // Close it immediately, otherwise clicking the close button causes it to slowly fade
                snackbar.ForceClose();
                return Task.CompletedTask;
            };
        });
    }

    protected bool IsSubmitButtonDisabled()
    {
        return _firstRenderDone && !IsValid;
    }
}