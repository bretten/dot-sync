using com.brettnamba.DotSync.Apps.Common.Components.Jobs;
using com.brettnamba.DotSync.Apps.Common.Components.Widgets.Overlay;
using Microsoft.Extensions.DependencyInjection;

namespace com.brettnamba.DotSync.Apps.Common.Startup;

public static class Dependencies
{
    /// <summary>
    /// Adds <see cref="JobDetailsProvider"/>
    /// </summary>
    /// <param name="services">Service collection</param>
    public static void AddJobDialog(this IServiceCollection services)
    {
        services.AddScoped<JobDetailsProvider>();
    }

    /// <summary>
    /// Adds overlay
    /// </summary>
    /// <param name="services">Service collection</param>
    public static void AddOverlay(this IServiceCollection services)
    {
        services.AddScoped<IOverlayService, SingleInstanceOverlayService>();
    }
}