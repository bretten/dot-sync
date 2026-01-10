namespace com.brettnamba.DotSync.FileSystem.Application.Jobs.Progress;

/// <summary>
/// Represents a reported log message
/// </summary>
/// <param name="JobId">Job ID</param>
/// <param name="Message">The log message that is being reported</param>
public sealed record ProgressLog(Guid JobId, string Message);