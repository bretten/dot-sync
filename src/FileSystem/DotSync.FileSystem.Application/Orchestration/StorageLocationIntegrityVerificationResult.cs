using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration;

/// <summary>
/// Verification result of <see cref="IStorageLocationIntegrityVerificationService"/>
/// </summary>
public readonly record struct StorageLocationIntegrityVerificationResult(
    FileSetIntegrityVerificationResult Result,
    string Report);