namespace com.brettnamba.DotSync.Apps.Common.Components.Widgets.Overlay;

/// <summary>
/// Implementation that assumes there is already an <see cref="Overlay"/> instance in the MainLayout. Any call
/// to show the Overlay will show that same instance and never create a new one.
/// </summary>
public sealed class SingleInstanceOverlayService : IOverlayService
{
    /// <summary>
    /// The single <see cref="Overlay"/> instance. Assign this in a component that lives in MainLayout during OnInitialized
    /// </summary>
    private Overlay Overlay { get; set; } = null!;

    /// <inheritdoc/>
    public Task<Overlay> Show()
    {
        Overlay.Show();
        return Task.FromResult(Overlay);
    }

    /// <inheritdoc/>
    public Task Hide()
    {
        Overlay.Close();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void RegisterOverlay(Overlay overlay)
    {
        Overlay = overlay;
    }
}