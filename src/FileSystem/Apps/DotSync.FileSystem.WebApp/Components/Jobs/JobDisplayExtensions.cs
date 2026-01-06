using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using MudBlazor;

namespace com.brettnamba.DotSync.FileSystem.WebApp.Components.Jobs;

/// <summary>
/// Extensions for displaying <see cref="IJob"/> on the UI
/// </summary>
public static class JobDisplayExtensions
{
    public static TimeSpan GetDuration(this IJob job, DateTimeOffset now)
    {
        var endTime = job.State != JobState.Done ? now : job.EndTime;
        return job.StartTime == DateTimeOffset.MinValue ? new TimeSpan(0) : endTime - job.StartTime;
    }

    public static string GetDurationString(this IJob job, DateTimeOffset now)
    {
        var duration = job.GetDuration(now);

        return duration.TotalMinutes < 1
            ? $"{Math.Round(duration.TotalSeconds, 2)} seconds"
            : $"{Math.Round(duration.TotalMinutes, 2)} minutes";
    }

    public static Tuple<string, Color> GetStateIcon(this IJob job)
    {
        return job.State switch
        {
            JobState.Queued => new Tuple<string, Color>(Icons.Material.Filled.AccessAlarm, Color.Default),
            JobState.InProgress => new Tuple<string, Color>(Icons.Material.Filled.PlayArrow, Color.Info),
            JobState.Done => new Tuple<string, Color>(Icons.Material.Filled.Check, Color.Success),
            _ => new Tuple<string, Color>(Icons.Material.Filled.Error, Color.Error)
        };
    }
}