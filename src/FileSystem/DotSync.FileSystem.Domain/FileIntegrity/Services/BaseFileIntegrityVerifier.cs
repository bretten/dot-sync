using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Services;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;

/// <summary>
/// Generalized file integrity verifier
/// </summary>
/// <param name="fileRepository">Stores the expected state of the files</param>
/// <param name="checksumGenerator">Generates checksums for files</param>
public abstract class BaseFileIntegrityVerifier(
    IFileRepository fileRepository,
    IFileChecksumGenerator checksumGenerator)
    : IFileIntegrityVerifier
{
    protected readonly IFileRepository FileRepository = fileRepository;
    protected readonly IFileChecksumGenerator ChecksumGenerator = checksumGenerator;

    public async Task Verify(FileSystemPath directoryPath)
    {
        var tasks = VerifyDirectory(directoryPath);
        await Task.WhenAll(tasks);
    }

    protected abstract IEnumerable<Task> VerifyDirectory(FileSystemPath directoryPath);
}