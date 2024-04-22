using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;

namespace DotSync.FileSystem.Application.Orchestration;

/// <summary>
/// Verification result of <see cref="IDirectoryVerificationService"/>
/// </summary>
public record DirectoryVerificationResult(StorageLocationIntegrityVerificationResult Result, string Report);