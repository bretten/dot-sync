using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;

namespace DotSync.Apps.WebApp.Demo.Mocks;

public sealed class MockFileIntegrityVerifierFactory : IFileIntegrityVerifierFactory
{
    private readonly IServiceProvider _serviceProvider;

    public MockFileIntegrityVerifierFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IFileIntegrityVerifier GetBy(StorageLocation storageLocation)
    {
        switch (storageLocation.Type)
        {
            case StorageLocationType.Local:
                return _serviceProvider.GetRequiredService<LocalFileSystemFileIntegrityVerifier>();
            case StorageLocationType.AmazonS3:
                return _serviceProvider.GetRequiredService<MockAmazonS3FileIntegrityVerifier>();
            default:
                throw new UnknownStorageLocationTypeForFactoryException($"Unknown type {storageLocation.Type}");
        }
    }

    private sealed class UnknownStorageLocationTypeForFactoryException(string message) : Exception(message);
}