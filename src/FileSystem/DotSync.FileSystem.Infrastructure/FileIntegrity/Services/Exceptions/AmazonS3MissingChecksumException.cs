namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services.Exceptions;

/// <summary>
/// Thrown when an S3 Object does not have a checksum
/// </summary>
public sealed class AmazonS3MissingChecksumException(string message) : Exception(message);