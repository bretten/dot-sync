using Amazon.S3;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;

public sealed class FileIntegrityVerifierFactory : IFileIntegrityVerifierFactory
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileChecksumGenerator _fileChecksumGenerator;
    private readonly IFileMetadataReader _metadataReader;
    private readonly IAmazonS3 _s3;

    public FileIntegrityVerifierFactory(IFileRepository fileRepository, IFileChecksumGenerator fileChecksumGenerator,
        IFileMetadataReader metadataReader, IAmazonS3 s3)
    {
        _fileRepository = fileRepository;
        _fileChecksumGenerator = fileChecksumGenerator;
        _metadataReader = metadataReader;
        _s3 = s3;
    }

    public IFileIntegrityVerifier GetBy(StorageLocation storageLocation)
    {
        switch (storageLocation.Type)
        {
            case StorageLocationType.Local:
                return new LocalFileSystemFileIntegrityVerifier(_fileRepository, _fileChecksumGenerator,
                    _metadataReader, storageLocation.Path);
            case StorageLocationType.AmazonS3:
                return new AmazonS3FileIntegrityVerifier(_fileRepository, _fileChecksumGenerator, _s3,
                    storageLocation.Path.Value);
            default:
                throw new UnknownStorageLocationTypeForFactoryException($"Unknown type {storageLocation.Type}");
        }
    }

    private sealed class UnknownStorageLocationTypeForFactoryException(string message) : Exception(message);
}