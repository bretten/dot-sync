using System.Security.Cryptography;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;

/// <summary>
/// Generates a SHA256 file checksum
/// </summary>
public sealed class Sha256FileChecksumGenerator : IFileChecksumGenerator
{
    /// <summary>
    /// <inheritdoc cref="IFileChecksumGenerator"/>
    /// </summary>
    public string GenerateChecksum(FileInfo fileInfo)
    {
        using SHA256 sha256 = SHA256.Create();
        using FileStream fileStream = fileInfo.OpenRead();

        // Beginning of the file stream
        fileStream.Position = 0;

        byte[] hashValue = sha256.ComputeHash(fileStream);

        return Convert.ToBase64String(hashValue, 0, hashValue.Length);
    }
}