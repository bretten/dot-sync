namespace com.brettnamba.DotSync.Common.Application.Jobs.Progress;

/// <summary>
/// Represents a percentage progress
/// </summary>
/// <param name="JobId">Job ID</param>
/// <param name="CurrentProgressUnits">The current progress (arbitrary units)</param>
/// <param name="TotalProgressUnits">The total progress (arbitrary units)</param>
public sealed record ProgressPercent(Guid JobId, double CurrentProgressUnits, double TotalProgressUnits)
{
    public double AsPercent() => (CurrentProgressUnits / TotalProgressUnits) * 100;
}