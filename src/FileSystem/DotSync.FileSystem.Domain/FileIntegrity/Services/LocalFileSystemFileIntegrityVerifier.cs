using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Services;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;

/// <summary>
/// Verifies the integrity of files on a local filesystem
/// <inheritdoc cref="BaseFileIntegrityVerifier" path="/param[@name='fileRepository']"/>
/// </summary>
public sealed class LocalFileSystemFileIntegrityVerifier(
    IFileRepository fileRepository,
    IFileChecksumGenerator checksumGenerator)
    : BaseFileIntegrityVerifier(fileRepository, checksumGenerator)
{
    protected override IEnumerable<Task> VerifyDirectory(FileSystemPath directoryPath)
    {
        return VerifyDirectory(new DirectoryInfo(directoryPath.Value), directoryPath);
    }

    private IEnumerable<Task> VerifyDirectory(DirectoryInfo directoryInfo, FileSystemPath rootDirectoryPath)
    {
        var entries = directoryInfo.EnumerateFileSystemInfos();
        var tasks = new List<Task>();
        foreach (var entry in entries)
        {
            switch (entry)
            {
                case FileInfo info:
                    tasks.Add(VerifyFile(info, rootDirectoryPath));
                    break;
                case DirectoryInfo info:
                    tasks.AddRange(VerifyDirectory(info, rootDirectoryPath));
                    break;
            }
        }

        return tasks;
    }

    private async Task VerifyFile(FileInfo fileInfo, FileSystemPath rootDirectoryPath)
    {
        // Generate the checksum of the file on the filesystem
        var checksum = ChecksumGenerator.GenerateChecksum(fileInfo);
        // Determine its relative path compared to the root directory
        var relativePath = Path.GetRelativePath(rootDirectoryPath.Value, fileInfo.FullName);

        // See if the file's checksum already exists
        var existingFileByChecksum = await FileRepository.GetFileByChecksum(checksum);
        if (existingFileByChecksum != null)
        {
            // The checksum already has a match. If the path matches, the file can be verified
            if (relativePath == existingFileByChecksum.Path.Value)
            {
                existingFileByChecksum.SetAsVerified();
                return;
            }

            // The checksum matched, but its path is out of date. Update the path and then verify the file
            existingFileByChecksum.UpdatePath(relativePath);
            existingFileByChecksum.SetAsVerified();
            await FileRepository.Update(existingFileByChecksum);
            return;
        }

        // The file could not be found via checksum, so check to see if the path is being used
        var existingFileByPath = await FileRepository.GetFileByPath(relativePath);
        if (existingFileByPath != null)
        {
            // The path was being used, so it could be a couple of cases
            throw new HashNotFoundException(
                "File was modified so has new hash OR The file at the DB path was deleted/moved and new file was added with the same path",
                fileInfo);
        }

        // The file could not be found via checksum or file path. It is a new file, so add it
        var newFile = new DotFile(Guid.NewGuid(), FileSystemPath.Create(relativePath),
            FileSha256Checksum.Create(checksum));
        newFile.SetAsVerified();
        await FileRepository.Add(newFile);
    }

    public class HashNotFoundException(string? message, FileInfo fileInfo) : Exception(message)
    {
        public FileInfo FileInfo { get; } = fileInfo;
    }
}