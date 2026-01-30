using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using com.brettnamba.DotSync.Common.Infrastructure.Aws;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.Services;

/// <summary>
/// Copies files to S3
/// </summary>
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

    /// <inheritdoc/>
    public async Task<bool> Exists(DotFile file, StorageLocation destination)
    {
        // The storage location is a S3 bucket, so get the bucket name
        var bucket = destination.Path.WithoutLeadingAndTrailingSlash;
        var key = file.Path.Value;
        return await Exists(bucket, key);
    }

    /// <inheritdoc/>
    public async Task<bool> CopyFile(StorageLocation source, DotFile file, StorageLocation destination,
        Action<long, long> updateUploadProgress)
    {
        // The storage location is a S3 bucket, so get the bucket name
        var bucket = destination.Path.WithoutLeadingAndTrailingSlash;
        var key = file.Path.Value;
        // The file that will be uploaded
        var fullFilePath = source.Path.ConcatenateFilePath(file.Path);

        var exists = await Exists(bucket, key);
        if (exists) return false;
        _logger.LogInformation($"Uploading {fullFilePath}");

        var fileInfo = new FileInfo(fullFilePath);

        if (fileInfo.Length >= SinglePartUploadMaxSize)
        {
            await MultiPartUpload(bucket: bucket, key: key, fileInfo, updateUploadProgress);
            return true;
        }

        await SinglePartUpload(bucket: bucket, key: key, fileInfo, updateUploadProgress);
        return true;
    }

    private async Task SinglePartUpload(string bucket, string key, FileInfo fileInfo,
        Action<long, long> updateUploadProgress)
    {
        var checksum = _fileChecksumGenerator.GenerateChecksum(fileInfo);

        // Change the config so that it forces single part upload by maximizing the point at which TransferUtility forces multipart upload
        // In the calling method, we already check for multipart upload so no need to actually use it here in SinglePartUpload
        var config = new TransferUtilityConfig
        {
            MinSizeBeforePartUpload = long.MaxValue
        };
        using var fileTransferUtility = new TransferUtility(_s3, config);
        var fileTransferUtilityRequest = new TransferUtilityUploadRequest
        {
            BucketName = bucket,
            Key = key,
            FilePath = fileInfo.FullName,
            ChecksumAlgorithm = ChecksumAlgorithm.SHA256,
            ChecksumSHA256 = checksum,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
            StorageClass = _storageClass
        };
        fileTransferUtilityRequest.Metadata.Add(Constants.Metadata.Keys.Sha256Checksum, checksum);

        // Notify progress changes using the callback
        fileTransferUtilityRequest.UploadProgressEvent += (s, e) =>
        {
            updateUploadProgress(e.TransferredBytes, e.TotalBytes);
        };

        await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
    }

    private async Task MultiPartUpload(string bucket, string key, FileInfo fileInfo,
        Action<long, long> updateUploadProgress)
    {
        using var fileTransferUtility = new TransferUtility(_s3);
        var fileTransferUtilityRequest = new TransferUtilityUploadRequest
        {
            BucketName = bucket,
            Key = key,
            FilePath = fileInfo.FullName,
            ChecksumAlgorithm = ChecksumAlgorithm.SHA256,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
            PartSize = 6291456, // 6 MB, the size of the parts uploaded
            // The SHA256 checksum cannot be set for a multipart upload because it is generated as a composite of all the files, so we will force a copy and a regeneration of the checksum on S3's side
            StorageClass = _storageClass
        };
        fileTransferUtilityRequest.Metadata.Add(Constants.Metadata.Keys.Sha256Checksum,
            _fileChecksumGenerator.GenerateChecksum(fileInfo));

        // Notify progress changes using the callback
        fileTransferUtilityRequest.UploadProgressEvent += (s, e) =>
        {
            updateUploadProgress(e.TransferredBytes, e.TotalBytes);
        };

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

    private async Task<bool> Exists(string bucket, string key)
    {
        var request = new GetObjectMetadataRequest()
        {
            BucketName = bucket,
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