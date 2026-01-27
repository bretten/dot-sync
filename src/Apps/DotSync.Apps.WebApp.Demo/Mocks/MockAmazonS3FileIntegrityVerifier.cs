using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;

namespace DotSync.Apps.WebApp.Demo.Mocks;

/// <summary>
/// Mock <see cref="IFileIntegrityVerifier"/>
/// </summary>
public sealed class MockAmazonS3FileIntegrityVerifier : BaseFileIntegrityVerifier
{
    private readonly MockS3Storage _mockS3Storage;
    private readonly JobExecutionContext _jobContext;

    public MockAmazonS3FileIntegrityVerifier(IFileRepository fileRepository, IFileChecksumGenerator checksumGenerator,
        ILogger<IFileIntegrityVerifier> logger, MockS3Storage mockS3Storage, JobExecutionContext jobContext) : base(
        fileRepository, checksumGenerator, logger)
    {
        _mockS3Storage = mockS3Storage;
        _jobContext = jobContext;
    }

    protected override Task<IEnumerable<FileIntegrityVerificationResult>> VerifyDirectory(
        StorageLocation storageLocation, string pathPrefix, IEnumerable<string> pathsToSkip)
    {
        var filesInStorage = _mockS3Storage.FilesFor(storageLocation);
        var skipPaths = pathsToSkip.ToList();

        var results = new List<FileIntegrityVerificationResult>();
        foreach (var file in filesInStorage)
        {
            if (!string.IsNullOrEmpty(pathPrefix) && !file.Path.Value.StartsWith(pathPrefix))
            {
                continue;
            }

            if (skipPaths.Any(x => file.Path.Value.StartsWith(x)))
            {
                Logger.LogInformation($"Skipping {file.Path.Value}");
                continue;
            }

            using (Logger.BeginScope(new List<KeyValuePair<string, object>>()
                   {
                       new(nameof(JobExecutionContext), _jobContext.Id)
                   }))
            {
                Logger.LogInformation($"Uploading {file.Path.Value}");
            }

            results.Add(FileIntegrityVerificationResult.Verified(file.Path, file.Sha256Checksum, file.Size));
        }

        return Task.FromResult(results.AsEnumerable());
    }
}