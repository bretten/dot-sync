using com.brettnamba.DotSync.FileSystem.Application.Maintenance;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Maintenance;

public sealed class FileStatusChecker : IFileStatusChecker
{
    /// <summary>
    /// Dir where synced files will be placed
    /// </summary>
    private const string SyncedFilesPath = "Synced";

    /// <summary>
    /// Dir where unsynced files will be placed
    /// </summary>
    private const string UnsyncedFilesPath = "Unsynced";

    private readonly IFileChecksumGenerator _fileChecksumGenerator;

    private readonly IFileRepository _fileRepository;

    public FileStatusChecker(IFileChecksumGenerator fileChecksumGenerator, IFileRepository fileRepository)
    {
        _fileChecksumGenerator = fileChecksumGenerator;
        _fileRepository = fileRepository;
    }

    /// <inheritdoc/>
    public async Task CheckFileStatus(string path)
    {
        var directoryInfo = new DirectoryInfo(path);
        var entries = directoryInfo.EnumerateFileSystemInfos();

        foreach (var entry in entries)
        {
            if (entry.Attributes.HasFlag(FileAttributes.Hidden) || entry is not FileInfo)
            {
                continue;
            }

            var checksum =
                FileSha256Checksum.Create(_fileChecksumGenerator.GenerateChecksum(new FileInfo(entry.FullName)));

            var exists = await _fileRepository.GetFileByChecksum(checksum);

            var destDir = exists != null ? SyncedFilesPath : UnsyncedFilesPath;
            var destPath = Path.Combine(path, destDir);
            Directory.CreateDirectory(destPath);

            File.Move(entry.FullName, Path.Combine(destPath, entry.Name));
        }
    }
}