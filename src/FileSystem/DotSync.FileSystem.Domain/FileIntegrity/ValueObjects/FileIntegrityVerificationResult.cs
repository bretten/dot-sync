using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;

/// <summary>
/// Represents the result of a file integrity check on a single file in <see cref="IFileIntegrityVerifier"/>
/// </summary>
/// <param name="Path">The file path</param>
/// <param name="Checksum">The file checksum</param>
/// <param name="Size">The file size</param>
/// <param name="IsVerified">True if the file integrity was verified, otherwise false</param>
public readonly record struct FileIntegrityVerificationResult(
    FileSystemPath Path,
    FileSha256Checksum Checksum,
    long Size,
    bool IsVerified)
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
        return new FileIntegrityVerificationResult(path, checksum, size, true);
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
        return new FileIntegrityVerificationResult(path, checksum, size, false);
    }
};