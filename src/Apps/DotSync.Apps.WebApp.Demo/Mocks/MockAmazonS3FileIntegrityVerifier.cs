using System.Diagnostics.CodeAnalysis;
using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;

namespace DotSync.Apps.WebApp.Demo.Mocks;

/// <summary>
/// Mock <see cref="IFileIntegrityVerifier"/>
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class MockAmazonS3FileIntegrityVerifier : BaseFileIntegrityVerifier
{
    private readonly MockS3Storage _mockS3Storage;
    private readonly JobExecutionContext _jobContext;
    private readonly IJobProgressReporter _jobProgressReporter;

    private readonly Random _random = new Random();

    public MockAmazonS3FileIntegrityVerifier(IFileRepository fileRepository, IFileChecksumGenerator checksumGenerator,
        ILogger<IFileIntegrityVerifier> logger, MockS3Storage mockS3Storage, JobExecutionContext jobContext,
        IJobProgressReporter jobProgressReporter) : base(fileRepository, checksumGenerator, logger)
    {
        _mockS3Storage = mockS3Storage;
        _jobContext = jobContext;
        _jobProgressReporter = jobProgressReporter;
    }

    protected override async Task<IEnumerable<FileIntegrityVerificationResult>> VerifyDirectory(
        StorageLocation storageLocation, string pathPrefix, IEnumerable<string> pathsToSkip)
    {
        var filesInStorage = _mockS3Storage.FilesFor(storageLocation);

        // Determine which to skip and which to validate
        var skipPaths = pathsToSkip.ToList();
        var toVerify = new List<DotFile>();
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

            toVerify.Add(file);
        }

        // Verify the remaining files
        var verified = 0; // Tracks how many have been verified
        var results = new List<FileIntegrityVerificationResult>();
        foreach (var file in toVerify)
        {
            using (Logger.BeginScope(new List<KeyValuePair<string, object>>()
                   {
                       new(nameof(JobExecutionContext), _jobContext.Id)
                   }))
            {
                Logger.LogInformation($"Verifying {file.Path.Value}");
            }

            // Simulate requests to S3
            await Task.Delay(TimeSpan.FromSeconds(_random.Next(1, 5)));

            results.Add(FileIntegrityVerificationResult.Verified(file.Path, file.Sha256Checksum, file.Size));

            verified++;
            _jobProgressReporter.ReportPercent(this, _jobContext.Id, verified, toVerify.Count);
        }

        return results.AsEnumerable();
    }
}