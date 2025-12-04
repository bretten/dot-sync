using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using com.brettnamba.DotSync.Common.Converters;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;

/// <summary>
/// The different types of <see cref="StorageLocation"/>
/// </summary>
[TypeConverter(typeof(EnumDisplayNameConverter))]
public enum StorageLocationType
{
    /// <summary>
    /// A local filesystem
    /// </summary>
    [Display(Name = "local")] Local,

    /// <summary>
    /// Amazon S3
    /// </summary>
    [Display(Name = "s3")] AmazonS3
}