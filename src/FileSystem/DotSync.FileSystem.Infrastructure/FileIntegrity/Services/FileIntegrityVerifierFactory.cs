using Amazon.S3;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;
using Microsoft.Extensions.Logging;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;

public sealed class FileIntegrityVerifierFactory : IFileIntegrityVerifierFactory
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileChecksumGenerator _fileChecksumGenerator;
    private readonly IFileMetadataReader _metadataReader;
    private readonly IAmazonS3 _s3;
    private readonly ILogger<IFileIntegrityVerifier> _logger;

    public FileIntegrityVerifierFactory(IFileRepository fileRepository, IFileChecksumGenerator fileChecksumGenerator,
        IFileMetadataReader metadataReader, IAmazonS3 s3, ILogger<IFileIntegrityVerifier> logger)
    {
        _fileRepository = fileRepository;
        _fileChecksumGenerator = fileChecksumGenerator;
        _metadataReader = metadataReader;
        _s3 = s3;
        _logger = logger;
    }

    public IFileIntegrityVerifier GetBy(StorageLocation storageLocation)
    {
        switch (storageLocation.Type)
        {
            case StorageLocationType.Local:
                return new LocalFileSystemFileIntegrityVerifier(_fileRepository, _fileChecksumGenerator, _logger,
                    _metadataReader, storageLocation.Path);
            case StorageLocationType.AmazonS3:
                return new AmazonS3FileIntegrityVerifier(_fileRepository, _fileChecksumGenerator, _logger, _s3,
                    storageLocation.Path.Value);
            default:
                throw new UnknownStorageLocationTypeForFactoryException($"Unknown type {storageLocation.Type}");
        }
    }

    private sealed class UnknownStorageLocationTypeForFactoryException(string message) : Exception(message);
}