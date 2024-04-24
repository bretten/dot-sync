namespace com.brettnamba.DotSync.Common.DateAndTme;

/// <summary>
/// <inheritdoc cref="IClock"/>
/// </summary>
public sealed class Clock : IClock
{
    public DateTimeOffset GetUtcNow()
    {
        return DateTimeOffset.UtcNow;
    }
}