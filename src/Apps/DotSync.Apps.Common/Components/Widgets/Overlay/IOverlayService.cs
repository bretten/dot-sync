namespace com.brettnamba.DotSync.Apps.Common.Components.Widgets.Overlay;

/// <summary>
/// Service that displays an <see cref="Overlay"/>
/// </summary>
public interface IOverlayService
{
    /// <summary>
    /// Displays an <see cref="Overlay"/>
    /// </summary>
    Task<Overlay> Show();

    /// <summary>
    /// Hides an <see cref="Overlay"/>
    /// </summary>
    Task Hide();

    /// <summary>
    /// Registers an <see cref="Overlay"/> with the service to be used for display
    /// </summary>
    /// <param name="overlay">The <see cref="Overlay"/> to register</param>
    void RegisterOverlay(Overlay overlay);
}