using System.Collections.Concurrent;
using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.Common.Infrastructure.Aws;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services.Exceptions;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;

/// <summary>
/// Verifies the integrity of files in an Amazon S3 bucket
/// </summary>
public sealed class AmazonS3FileIntegrityVerifier : BaseFileIntegrityVerifier
{
    /// <summary>
    /// Amazon S3 client
    /// </summary>
    private readonly IAmazonS3 _s3;

    /// <summary>
    /// Execution context for the current job
    /// </summary>
    private readonly JobExecutionContext _jobContext;

    /// <summary>
    /// Reports job progress
    /// </summary>
    private readonly IJobProgressReporter _jobProgressReporter;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fileRepository">Stores the expected state of the files</param>
    /// <param name="fileChecksumGenerator">Generates checksums for files</param>
    /// <param name="logger">Logger</param>
    /// <param name="s3">Amazon S3 client</param>
    /// <param name="jobContext">Execution context for the current job</param>
    /// <param name="jobProgressReporter">Reports job progress</param>
    public AmazonS3FileIntegrityVerifier(IFileRepository fileRepository, IFileChecksumGenerator fileChecksumGenerator,
        ILogger<IFileIntegrityVerifier> logger, IAmazonS3 s3, JobExecutionContext jobContext,
        IJobProgressReporter jobProgressReporter) : base(fileRepository, fileChecksumGenerator, logger)
    {
        _s3 = s3;
        _jobContext = jobContext;
        _jobProgressReporter = jobProgressReporter;
    }

    /// <summary>
    /// Max number of keys in a S3 ListObjectsV2 request
    /// </summary>
    private const int MaxKeys = 1000;

    /// <summary>
    /// Verifies the directory, in this case, an Amazon S3 bucket
    /// </summary>
    /// <param name="storageLocation">The storage location to verify</param>
    /// <param name="pathPrefix">The prefix within the Amazon S3 bucket to verify</param>
    /// <param name="pathsToSkip">Paths to skip</param>
    /// <returns>Verification results for each file within the bucket</returns>
    /// <exception cref="AmazonS3ListObjectsPaginationException">Thrown if paginating over the keys in the bucket returns a non-OK status</exception>
    protected override async Task<IEnumerable<FileIntegrityVerificationResult>> VerifyDirectory(
        StorageLocation storageLocation, string pathPrefix, IEnumerable<string> pathsToSkip)
    {
        string bucket = storageLocation.Path.WithoutLeadingAndTrailingSlash;
        // Will hold the individual S3 Object verification results
        var results = new ConcurrentBag<FileIntegrityVerificationResult>();

        // Keep track of how many were verified
        var verified = 0;

        // Paginate over all the S3 objects in the bucket
        var request = new ListObjectsV2Request
        {
            BucketName = bucket,
            MaxKeys = MaxKeys,
            Prefix = pathPrefix
        };
        var paginator = _s3.Paginators.ListObjectsV2(request);
        await foreach (var response in paginator.Responses)
        {
            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new AmazonS3ListObjectsPaginationException(
                    $"ListObjectsV2 pagination returned {response.HttpStatusCode}");
            }

            // For each page of S3 objects, verify their checksums
            await Parallel.ForEachAsync(response.S3Objects, async (s3Object, token) =>
            {
                // See if the object should be skipped
                if (pathsToSkip.Any(x => s3Object.Key.StartsWith(x)))
                {
                    Logger.LogInformation($"Skipping {s3Object.Key}");
                    return;
                }

                // Verify the object
                Logger.LogInformation($"Verifying {s3Object.Key}");
                var result = await VerifyS3Object(bucket, s3Object);

                // Update progress
                Interlocked.Increment(ref verified);
                _jobProgressReporter.ReportPercent(this, _jobContext.Id, verified, response.S3Objects.Count);

                results.Add(result);
            });
        }

        return results;
    }

    /// <summary>
    /// Verifies a S3 Object by its checksum
    /// </summary>
    /// <param name="bucket">The containing S3 bucket</param>
    /// <param name="s3Object">The S3 Object to verify</param>
    /// <returns>The result of the verification</returns>
    private async Task<FileIntegrityVerificationResult> VerifyS3Object(string bucket, S3Object s3Object)
    {
        // Get the S3 Object's checksum
        var s3Checksum = FileSha256Checksum.Create(await GetS3ObjectSha256Checksum(bucket, s3Object.Key));

        // The path of the S3 Object
        var s3Path = FileSystemPath.Create(s3Object.Key);

        // See if the S3 Object's checksum matches a synced file
        var existingFileByChecksum = await FileRepository.GetFileByChecksum(s3Checksum);
        if (existingFileByChecksum != null && s3Path == existingFileByChecksum.Path)
        {
            // The checksum and path matched, so the file has been verified
            return FileIntegrityVerificationResult.Verified(s3Path, s3Checksum, s3Object.Size ?? 0);
        }

        // The S3 Object could not be verified against any synced file
        return FileIntegrityVerificationResult.Unverified(s3Path, s3Checksum, s3Object.Size ?? 0);
    }

    /// <summary>
    /// Gets the SHA256 checksum of the S3 Object specified by the key
    ///
    /// S3 checksum needs to be present. If the s3 and metadata (user defined) checksums are not equal, that means
    /// the file was uploaded in parts and the checksum is based on all parts. So use the fallback checksum in
    /// the metadata which is based on the whole file.
    /// </summary>
    /// <param name="bucket">The containing S3 bucket</param>
    /// <param name="key">The key of the S3 Object</param>
    /// <returns>The SHA256 checksum</returns>
    /// <exception cref="AmazonS3MissingChecksumException">Thrown if the checksum does not exist</exception>
    private async Task<string> GetS3ObjectSha256Checksum(string bucket, string key)
    {
        var metaData = await GetObjectMetadata(bucket, key);
        var s3Checksum = metaData?.ChecksumSHA256;
        var metadataChecksum = metaData?.Metadata[Constants.Metadata.Keys.Sha256Checksum];

        if (string.IsNullOrWhiteSpace(s3Checksum))
        {
            throw new AmazonS3MissingChecksumException($"No checksum for S3 Object: {key}");
        }

        var equal = s3Checksum == metadataChecksum;

        return !equal && !string.IsNullOrWhiteSpace(metadataChecksum) ? metadataChecksum : s3Checksum;
    }

    /// <summary>
    /// Gets the metadata of the S3 Object specified by the key
    /// </summary>
    /// <param name="bucket">The containing S3 bucket</param>
    /// <param name="key">The key of the S3 Object</param>
    /// <returns>The S3 Object metadata</returns>
    private async Task<GetObjectMetadataResponse?> GetObjectMetadata(string bucket, string key)
    {
        var request = new GetObjectMetadataRequest()
        {
            BucketName = bucket,
            Key = key,
            ChecksumMode = ChecksumMode.ENABLED
        };

        return await _s3.GetObjectMetadataAsync(request);
    }
}