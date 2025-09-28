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
    public async Task<string> GenerateChecksum(FileInfo fileInfo)
    {
        using var sha256 = SHA256.Create();
        await using var fileStream = fileInfo.OpenRead();

        // Beginning of the file stream
        fileStream.Position = 0;

        var hashValue = await sha256.ComputeHashAsync(fileStream);

        return Convert.ToBase64String(hashValue, 0, hashValue.Length);
    }
}