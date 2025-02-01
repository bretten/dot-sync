using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Aws;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.Services;

public sealed class AmazonS3FileCopier : IFileCopier
{
    private readonly IFileChecksumGenerator _fileChecksumGenerator;

    private readonly IAmazonS3 _s3;

    private readonly S3StorageClass _storageClass;

    private readonly ILogger<AmazonS3FileCopier> _logger;

    private const long SinglePartUploadMaxSize = 5368709120;

    public AmazonS3FileCopier(IFileChecksumGenerator fileChecksumGenerator, IAmazonS3 s3, S3StorageClass storageClass,
        ILogger<AmazonS3FileCopier> logger)
    {
        _fileChecksumGenerator = fileChecksumGenerator;
        _s3 = s3;
        _storageClass = storageClass;
        _logger = logger;
    }

    public async Task CopyFile(FileSystemPath sourcePath, FileSystemPath sourceFile, FileSystemPath destination)
    {
        var exists = await Exists(destination.Value, sourceFile.Value);
        if (exists) return;
        _logger.LogInformation($"Uploading {sourcePath.Value}/{sourceFile.Value}");

        var fileInfo =
            new FileInfo(FileSystemPath.Create(Path.Combine(sourcePath.Value, sourceFile.Value), true).Value);

        if (fileInfo.Length >= SinglePartUploadMaxSize)
        {
            await MultiPartUpload(sourceFile, destination, fileInfo);
            return;
        }

        await SinglePartUpload(sourceFile, destination, fileInfo);
    }

    private async Task SinglePartUpload(FileSystemPath sourceFile, FileSystemPath destination, FileInfo fileInfo)
    {
        var request = new PutObjectRequest
        {
            BucketName = destination.Value,
            Key = sourceFile.Value,
            FilePath = fileInfo.FullName,
            ChecksumAlgorithm = ChecksumAlgorithm.SHA256,
            ChecksumSHA256 = _fileChecksumGenerator.GenerateChecksum(fileInfo),
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
            StorageClass = _storageClass
        };
        request.Metadata.Add(Constants.Metadata.Keys.Sha256Checksum, _fileChecksumGenerator.GenerateChecksum(fileInfo));

        await _s3.PutObjectAsync(request);
    }

    private async Task MultiPartUpload(FileSystemPath sourceFile, FileSystemPath destination, FileInfo fileInfo)
    {
        using var fileTransferUtility = new TransferUtility(_s3);
        var fileTransferUtilityRequest = new TransferUtilityUploadRequest
        {
            BucketName = destination.Value,
            Key = sourceFile.Value,
            FilePath = fileInfo.FullName,
            ChecksumAlgorithm = ChecksumAlgorithm.SHA256,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
            PartSize = 6291456, // 6 MB, the size of the parts uploaded
            // The SHA256 checksum cannot be set for a multipart upload because it is generated as a composite of all the files, so we will force a copy and a regeneration of the checksum on S3's side
            StorageClass = _storageClass
        };
        fileTransferUtilityRequest.Metadata.Add(Constants.Metadata.Keys.Sha256Checksum,
            _fileChecksumGenerator.GenerateChecksum(fileInfo));

        await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);

        // Force a copy and regeneration of the checksum on s3's side. NOTE: This doesn't work because the same logic applies to copying as well as uploading, copy file size can't exceed the upload size
        //await UpdateStorageClass(destination.Value, sourceFile.Value);
    }

    /// <summary>
    /// In order to update the storage class, the file needs to be copied
    /// </summary>
    private async Task<CopyObjectResponse> UpdateStorageClass(string bucketName, string key)
    {
        var request = new CopyObjectRequest
        {
            SourceBucket = bucketName,
            SourceKey = key,
            DestinationBucket = bucketName,
            DestinationKey = key,
            StorageClass = _storageClass,
            ChecksumAlgorithm = ChecksumAlgorithm.SHA256,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
            MetadataDirective = S3MetadataDirective.COPY
        };
        return await _s3.CopyObjectAsync(request);
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