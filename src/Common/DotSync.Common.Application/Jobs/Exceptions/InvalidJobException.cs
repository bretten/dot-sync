namespace com.brettnamba.DotSync.Common.Application.Jobs.Exceptions;

/// <summary>
/// Thrown when a job is considered invalid
/// </summary>
public sealed class InvalidJobException(string message) : Exception(message);