using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;

/// <summary>
/// Verifies the integrity of files on a local filesystem
/// </summary>
public sealed class LocalFileSystemFileIntegrityVerifier(
    IFileRepository fileRepository,
    IFileChecksumGenerator checksumGenerator,
    IFileMetadataReader metadataReader)
    : BaseFileIntegrityVerifier(fileRepository, checksumGenerator)
{
    /// <summary>
    /// Verifies the integrity of all files within the specified directory
    /// </summary>
    /// <param name="directoryPath">The path to the directory that will be verified</param>
    /// <returns>Verification results for each file within the directory</returns>
    protected override IEnumerable<Task<FileIntegrityVerificationResult>> VerifyDirectory(FileSystemPath directoryPath)
    {
        return VerifyDirectory(new DirectoryInfo(directoryPath.Value), directoryPath);
    }

    /// <summary>
    /// Verifies the integrity of all files within the specified directory
    /// </summary>
    /// <param name="directoryInfo">The directory to verify</param>
    /// <param name="rootDirectoryPath">The original root directory that is being verified</param>
    /// <returns>Verification results for each file within the directory</returns>
    private IEnumerable<Task<FileIntegrityVerificationResult>> VerifyDirectory(DirectoryInfo directoryInfo,
        FileSystemPath rootDirectoryPath)
    {
        var entries = directoryInfo.EnumerateFileSystemInfos();
        var tasks = new List<Task<FileIntegrityVerificationResult>>();
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

    /// <summary>
    /// Verifies the integrity of a single file
    /// </summary>
    /// <param name="fileInfo">The file</param>
    /// <param name="rootDirectoryPath">The original root directory that is being verified</param>
    /// <returns>Verification result of the file</returns>
    private async Task<FileIntegrityVerificationResult> VerifyFile(FileInfo fileInfo, FileSystemPath rootDirectoryPath)
    {
        // Generate the checksum of the file on the filesystem
        var checksum = ChecksumGenerator.GenerateChecksum(fileInfo);
        // Determine its relative path compared to the root directory
        var relativePath = FileSystemPath.Create(Path.GetRelativePath(rootDirectoryPath.Value, fileInfo.FullName),
            replaceBackslashes: OperatingSystem.IsWindows());

        // See if the file's checksum already exists
        var existingFileByChecksum = await FileRepository.GetFileByChecksum(FileSha256Checksum.Create(checksum));
        if (existingFileByChecksum != null)
        {
            // The checksum matched, but its path is out of date. Update the path and then verify the file
            if (relativePath != existingFileByChecksum.Path)
            {
                existingFileByChecksum.UpdatePath(relativePath);
            }

            // The checksum and path match, so the file can be verified
            existingFileByChecksum.SetAsVerified();
            await FileRepository.Update(existingFileByChecksum);
            return FileIntegrityVerificationResult.Verified(existingFileByChecksum.Path,
                existingFileByChecksum.Sha256Checksum, fileInfo.Length);
        }

        // The file could not be found via checksum, so check to see if the path is being used
        var existingFileByPath = await FileRepository.GetFileByPath(relativePath);
        if (existingFileByPath != null)
        {
            // The path was being used, so it could be a couple of cases
            return FileIntegrityVerificationResult.Unverified(existingFileByPath.Path,
                existingFileByPath.Sha256Checksum, fileInfo.Length);
        }

        // File creation time (or best estimation)
        var fileCreation = metadataReader.ReadFileCreationDate(FileSystemPath.Create(fileInfo.FullName));

        // The file could not be found via checksum or file path. It is a new file, so add it
        var newFile = new DotFile(Guid.NewGuid(), relativePath, FileSha256Checksum.Create(checksum), fileInfo.Length,
            fileCreation);
        newFile.SetAsVerified();
        await FileRepository.Add(newFile);
        return FileIntegrityVerificationResult.Verified(newFile.Path, newFile.Sha256Checksum, fileInfo.Length);
    }
}