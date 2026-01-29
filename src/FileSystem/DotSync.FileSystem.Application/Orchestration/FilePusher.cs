using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.Common.Application.Jobs.Execution;
using com.brettnamba.DotSync.Common.Application.Jobs.Extensions;
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
    public async Task<IEnumerable<DotFile>> PushFilesInStorage(StorageLocation source, string pathPrefix,
        long uploadLimitMb, StorageLocation destination)
    {
        // Get unsynced files
        var files = (await _fileRepository.GetUnsyncedFiles(destination.Id, pathPrefix)).ToList();
        // Determine what can be uploaded with the upload limit
        var uploadedBytes = 0L;
        var uploadLimitBytes = uploadLimitMb * 1000000; // Use MB, not MiB since that is the user-facing value
        var filesToPush = new List<DotFile>();
        foreach (var file in files)
        {
            // If it exists, no further action needed. Also, don't count it in the total limit
            var exists = await _fileCopier.Exists(file, destination);
            if (exists) continue;

            // Loose limit for now, just iterate until the rough limit is reached
            var projectedTotalUploadAmount = uploadedBytes + file.Size;
            if (uploadLimitMb != 0 && projectedTotalUploadAmount > uploadLimitBytes)
            {
                _logger.LogInformation(
                    $"Skipping {file.Path.Value} ({file.Size}) because it would put it over the limit of {uploadLimitBytes} bytes. Current upload size: {uploadedBytes}");
                continue;
            }

            // Track that the file will be uploaded
            filesToPush.Add(file);
            // Update the total amount to be uploaded
            uploadedBytes += file.Size;
        }

        // The total bytes of all the files that will be uploaded
        var totalBytesToUpload = uploadedBytes;
        // Reset the counter used to measure the total to track the current progress during actual upload
        uploadedBytes = 0;

        // Upload files
        var pushedFiles = new List<DotFile>();
        foreach (var file in filesToPush)
        {
            // Upload the file
            await _fileCopier.CopyFile(source, file, destination);

            // Add the synced file to the domain
            pushedFiles.Add(file);
            await _fileRepository.AddSyncedFile(file.Id, destination.Id);

            // Update progress
            uploadedBytes += file.Size;
            _jobProgressReporter.ReportPercent(this, _jobContext.Id, uploadedBytes, totalBytesToUpload);
            _logger.LogWithScope($"Uploaded {file.Path.Value}", _jobContext);
        }

        return pushedFiles;
    }
}