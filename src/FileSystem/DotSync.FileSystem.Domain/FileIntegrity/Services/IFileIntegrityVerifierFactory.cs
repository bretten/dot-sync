using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;

namespace com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;

public interface IFileIntegrityVerifierFactory
{
    IFileIntegrityVerifier GetBy(StorageLocation storageLocation);
}