using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;

namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration;

/// <summary>
/// Verification result of <see cref="IStorageLocationIntegrityVerificationService"/>
/// </summary>
public readonly record struct StorageLocationIntegrityVerificationResult(
    StorageLocation StorageLocation,
    FileSetIntegrityVerificationResult Result,
    string Report);