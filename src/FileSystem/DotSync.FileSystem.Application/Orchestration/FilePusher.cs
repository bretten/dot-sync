using com.brettnamba.DotSync.FileSystem.Application.Jobs.Contracts;
using com.brettnamba.DotSync.FileSystem.Application.Jobs.Execution;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration;

/// <inheritdoc/>
public sealed class FilePusher : IFilePusher
{
    private readonly IFileRepository _fileRepository;

    private readonly IFileCopier _fileCopier;

    private readonly JobExecutionContext _jobContext;

    private readonly IJobProgressReporter _jobProgressReporter;

    private readonly ILogger<FilePusher> _logger;

    public FilePusher(IFileRepository fileRepository, IFileCopier fileCopier, JobExecutionContext jobContext,
        IJobProgressReporter jobProgressReporter, ILogger<FilePusher> logger)
    {
        _fileRepository = fileRepository;
        _fileCopier = fileCopier;
        _jobContext = jobContext;
        _jobProgressReporter = jobProgressReporter;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DotFile>> PushFilesByPath(StorageLocation source, string pathPrefix,
        StorageLocation destination)
    {
        var files = (await _fileRepository.GetFilesByPathPrefix(pathPrefix)).ToList();
        var totalBytes = files.Sum(f => f.Size);

        var pushedFiles = new List<DotFile>();
        var bytesPushed = 0L;
        foreach (var file in files)
        {
            var pushed = await _fileCopier.CopyFile(source, file, destination);
            bytesPushed += file.Size;
            _jobProgressReporter.ReportPercent(this, _jobContext.Id, bytesPushed, totalBytes);

            if (!pushed) continue;
            using (_logger.BeginScope(new List<KeyValuePair<string, object>>()
                   {
                       new(nameof(JobExecutionContext), _jobContext.Id)
                   }))
            {
                _logger.LogInformation($"Uploaded {file.Path.Value}");
            }

            pushedFiles.Add(file);
            await _fileRepository.AddSyncedFile(file.Id, destination.Id);
        }

        return pushedFiles;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DotFile>> PushFilesInStorage(StorageLocation source, string pathPrefix,
        long uploadLimitMb, StorageLocation destination)
    {
        var files = (await _fileRepository.GetUnsyncedFiles(destination.Id, pathPrefix)).ToList();

        var pushedFiles = new List<DotFile>();
        var uploadedBytes = 0L;
        var uploadLimitBytes = uploadLimitMb * 1000000; // Use MB, not MiB since that is the user-facing value
        foreach (var file in files)
        {
            // Loose limit for now, just iterate until the rough limit is reached
            var projectedTotalUploadAmount = uploadedBytes + file.Size;
            if (uploadLimitMb != 0 && projectedTotalUploadAmount > uploadLimitBytes)
            {
                _logger.LogInformation(
                    $"Skipping {file.Path.Value} ({file.Size}) because it would put it over the limit of {uploadLimitBytes} bytes. Current upload size: {uploadedBytes}");
                continue;
            }

            var pushed = await _fileCopier.CopyFile(source, file, destination);
            if (!pushed) continue;
            using (_logger.BeginScope(new List<KeyValuePair<string, object>>()
                   {
                       new(nameof(JobExecutionContext), _jobContext.Id)
                   }))
            {
                _logger.LogInformation($"Uploaded {file.Path.Value}");
            }

            pushedFiles.Add(file);
            await _fileRepository.AddSyncedFile(file.Id, destination.Id);
            uploadedBytes += file.Size;
        }

        return pushedFiles;
    }
}