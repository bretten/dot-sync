using Amazon.S3;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services.Exceptions;

/// <summary>
/// Thrown when <see cref="IAmazonS3.Paginators"/> for ListObjects returns a non-OK status code
/// </summary>
public sealed class AmazonS3ListObjectsPaginationException(string message) : Exception(message);