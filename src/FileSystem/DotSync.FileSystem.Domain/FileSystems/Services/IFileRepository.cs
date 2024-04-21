using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;

public interface IFileRepository
{
    Task Add(DotFile file);
    Task Update(DotFile file);
    Task<DotFile?> GetFileByChecksum(string hash);
    Task<DotFile?> GetFileByPath(string path);
    Task<IEnumerable<DotFile>> GetUnverifiedFiles(FileSystemPath path);
}