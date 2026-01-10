using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace com.brettnamba.DotSync.FileSystem.WebApp.Components.Jobs.Forms;

public abstract class BaseForm : ComponentBase
{
    [CascadingParameter] protected IMudDialogInstance? MudDialog { get; set; }

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

    protected virtual Task Submit()
    {
        if (MudDialog != null)
        {
            MudDialog.Close();
        }

        return Task.CompletedTask;
    }

    protected bool IsSubmitButtonDisabled()
    {
        return _firstRenderDone && !IsValid;
    }
}