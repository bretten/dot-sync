using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;

namespace DotSync.Apps.WebApp.Demo.Mocks;

/// <summary>
/// Mock <see cref="IFileCopier"/>
/// </summary>
public sealed class MockAmazonS3FileCopier : IFileCopier
{
    private readonly Random _random = new Random();

    private readonly MockS3Storage _mockS3Storage;

    private readonly JobExecutionContext _jobContext;

    private readonly ILogger<MockAmazonS3FileCopier> _logger;

    public MockAmazonS3FileCopier(MockS3Storage mockS3Storage, JobExecutionContext jobContext,
        ILogger<MockAmazonS3FileCopier> logger)
    {
        _mockS3Storage = mockS3Storage;
        _jobContext = jobContext;
        _logger = logger;
    }

    public Task<bool> Exists(DotFile file, StorageLocation destination)
    {
        return Task.FromResult(_mockS3Storage.Exists(destination, file));
    }

    public async Task<bool> CopyFile(StorageLocation source, DotFile file, StorageLocation destination)
    {
        // Simulate upload
        using (_logger.BeginScope(new List<KeyValuePair<string, object>>()
               {
                   new(nameof(JobExecutionContext), _jobContext.Id)
               }))
        {
            _logger.LogInformation($"Uploading {file.Path.Value}");
        }

        await Task.Delay(TimeSpan.FromSeconds(_random.Next(1, 5)));

        // File has been uploaded
        _mockS3Storage.AddFile(destination, file);
        return true;
    }
}