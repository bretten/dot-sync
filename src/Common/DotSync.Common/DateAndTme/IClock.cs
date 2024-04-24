namespace com.brettnamba.DotSync.Common.DateAndTme;

/// <summary>
/// Defines a clock that gets the current time
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets UTC now
    /// </summary>
    DateTimeOffset GetUtcNow();
}