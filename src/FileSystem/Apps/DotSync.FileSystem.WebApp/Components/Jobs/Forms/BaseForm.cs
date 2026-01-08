using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace com.brettnamba.DotSync.FileSystem.WebApp.Components.Jobs.Forms;

public abstract class BaseForm : ComponentBase
{
    [CascadingParameter] protected IMudDialogInstance? MudDialog { get; set; }

    protected MudForm Form = null!;
    protected bool IsValid;
    protected string[] Errors = [];

    protected virtual Task Submit()
    {
        if (MudDialog != null)
        {
            MudDialog.Close();
        }

        return Task.CompletedTask;
    }
}