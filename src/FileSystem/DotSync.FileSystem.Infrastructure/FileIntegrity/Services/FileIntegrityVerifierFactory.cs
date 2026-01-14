using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;

/// <summary>
/// Factory for <see cref="IFileIntegrityVerifier"/>
/// </summary>
public sealed class FileIntegrityVerifierFactory : IFileIntegrityVerifierFactory
{
    /// <summary>
    /// Resolves the <see cref="IFileIntegrityVerifier"/> based on the current scope
    ///
    /// NOTE: I prefer injecting the service provider over the other option of manually resolving each dependency
    /// required by all the implementations of <see cref="IFileIntegrityVerifier"/> and passing those dependencies
    /// to the new instances.
    ///
    /// <see cref="IServiceScopeFactory"/> doesn't work here because this factory is resolved within a scope and all
    /// services need to share that scope
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    public FileIntegrityVerifierFactory(IServiceProvider serviceProvider)
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
                return _serviceProvider.GetRequiredService<AmazonS3FileIntegrityVerifier>();
            default:
                throw new UnknownStorageLocationTypeForFactoryException($"Unknown type {storageLocation.Type}");
        }
    }

    private sealed class UnknownStorageLocationTypeForFactoryException(string message) : Exception(message);
}