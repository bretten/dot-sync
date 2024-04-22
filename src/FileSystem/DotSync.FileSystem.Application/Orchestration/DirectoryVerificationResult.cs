using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration;

/// <summary>
/// Verification result of <see cref="IDirectoryVerificationService"/>
/// </summary>
public record DirectoryVerificationResult(StorageLocationIntegrityVerificationResult Result, string Report);