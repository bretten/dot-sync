using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;

namespace com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;

/// <summary>
/// The different types of <see cref="StorageLocation"/>
/// </summary>
public enum StorageLocationType
{
    /// <summary>
    /// A local filesystem
    /// </summary>
    Local,

    /// <summary>
    /// Amazon S3
    /// </summary>
    AmazonS3
}