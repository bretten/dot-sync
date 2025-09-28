namespace com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Enums;

public enum FileIntegrityStatus
{
    /// <summary>
    /// The file has been verified
    /// </summary>
    Verified,

    /// <summary>
    /// The file's checksum does not match
    /// </summary>
    Unverified,

    /// <summary>
    /// The file's path has changed
    /// </summary>
    Moved,

    /// <summary>
    /// The file is new
    /// </summary>
    New,

    /// <summary>
    /// The file no longer exists
    /// </summary>
    Missing
}