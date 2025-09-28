using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Enums;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;

/// <summary>
/// Represents the result of a file integrity check on a single file in <see cref="IFileIntegrityVerifier"/>
/// </summary>
/// <param name="Path">The file path</param>
/// <param name="Checksum">The file checksum</param>
/// <param name="Size">The file size</param>
/// <param name="Status">The verification status</param>
/// <param name="FileCreated">The metadata for the file creation date</param>
public readonly record struct FileIntegrityVerificationResult(
    FileSystemPath Path,
    FileSha256Checksum Checksum,
    long Size,
    FileIntegrityStatus Status,
    DateTime? FileCreated)
{
    /// <summary>
    /// Creates a verified result
    /// </summary>
    /// <param name="path">The file path</param>
    /// <param name="checksum">The file checksum</param>
    /// <param name="size">The file size</param>
    /// <returns>Verified result</returns>
    public static FileIntegrityVerificationResult Verified(FileSystemPath path, FileSha256Checksum checksum, long size)
    {
        return new FileIntegrityVerificationResult(path, checksum, size, FileIntegrityStatus.Verified, null);
    }

    /// <summary>
    /// Creates an unverified result
    /// </summary>
    /// <param name="path">The file path</param>
    /// <param name="checksum">The file checksum</param>
    /// <param name="size">The file size</param>
    /// <returns>Unverified result</returns>
    public static FileIntegrityVerificationResult Unverified(FileSystemPath path, FileSha256Checksum checksum,
        long size)
    {
        return new FileIntegrityVerificationResult(path, checksum, size, FileIntegrityStatus.Unverified, null);
    }

    /// <summary>
    /// Represents a new file
    /// </summary>
    /// <param name="path">The file path</param>
    /// <param name="checksum">The file checksum</param>
    /// <param name="size">The file size</param>
    /// <param name="creationDate">The metadata for the file creation date</param>
    /// <returns>The new file</returns>
    public static FileIntegrityVerificationResult New(FileSystemPath path, FileSha256Checksum checksum,
        long size, DateTime creationDate)
    {
        return new FileIntegrityVerificationResult(path, checksum, size, FileIntegrityStatus.New, creationDate);
    }

    public static FileIntegrityVerificationResult Moved(FileSystemPath path, FileSha256Checksum checksum,
        long size)
    {
        return new FileIntegrityVerificationResult(path, checksum, size, FileIntegrityStatus.Moved, null);
    }

    public static FileIntegrityVerificationResult Missing(FileSystemPath path, FileSha256Checksum checksum,
        long size)
    {
        return new FileIntegrityVerificationResult(path, checksum, size, FileIntegrityStatus.Missing, null);
    }
};