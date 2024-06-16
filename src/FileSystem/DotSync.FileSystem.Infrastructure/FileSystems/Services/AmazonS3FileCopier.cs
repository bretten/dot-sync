using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.Services;

public sealed class AmazonS3FileCopier : IFileCopier
{
    private readonly IFileChecksumGenerator _fileChecksumGenerator;

    private readonly IAmazonS3 _s3;

    public AmazonS3FileCopier(IFileChecksumGenerator fileChecksumGenerator, IAmazonS3 s3)
    {
        _fileChecksumGenerator = fileChecksumGenerator;
        _s3 = s3;
    }

    public async Task CopyFile(FileSystemPath sourcePath, FileSystemPath sourceFile, FileSystemPath destination)
    {
        var exists = await Exists(destination.Value, sourceFile.Value);
        if (exists) return;

        var fileInfo =
            new FileInfo(FileSystemPath.Create(Path.Combine(sourcePath.Value, sourceFile.Value), true).Value);
        var request = new PutObjectRequest
        {
            BucketName = destination.Value,
            Key = sourceFile.Value,
            FilePath = fileInfo.FullName,
            ChecksumAlgorithm = ChecksumAlgorithm.SHA256,
            ChecksumSHA256 = _fileChecksumGenerator.GenerateChecksum(fileInfo),
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
        };

        await _s3.PutObjectAsync(request);
    }

    private async Task<bool> Exists(string bucketName, string key)
    {
        var request = new GetObjectMetadataRequest()
        {
            BucketName = bucketName,
            Key = key,
            ChecksumMode = ChecksumMode.ENABLED
        };

        try
        {
            var response = await _s3.GetObjectMetadataAsync(request);
            return response != null && response.HttpStatusCode == HttpStatusCode.OK;
        }
        catch (AmazonS3Exception)
        {
            return false;
        }
    }
}