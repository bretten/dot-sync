using System.Collections.Immutable;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;

namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration;

/// <summary>
/// Verification result of <see cref="IStorageLocationIntegrityVerificationService"/>
/// </summary>
public readonly record struct StorageLocationIntegrityVerificationResult(
    StorageLocation StorageLocation,
    FileSetIntegrityVerificationResult Result,
    string Report)
{
    public Dictionary<string, IReadOnlyList<string[]>> GenerateReport(string storageType, string storagePath,
        string verifyPath, string pathsToSkip)
    {
        var verified = new List<string[]>() { new string[] { "Verified Count", Result.TotalVerified.ToString() } };
        var unverified = Result.Unverified.Select(x => (string[])[x.Path.Value, x.Checksum.Value, x.Size.ToString()])
            .ToImmutableList();
        var moved = Result.Moved.Select(x => (string[])[x.Path.Value]).ToImmutableList();
        var missing = Result.Missing.Select(x => (string[])[x.Path.Value]).ToImmutableList();
        var newFiles = Result.New.Select(x => (string[])[x.Path.Value]).ToImmutableList();
        return new Dictionary<string, IReadOnlyList<string[]>>()
        {
            { "Storage Type", new List<string[]>() { new[] { storageType } }.ToImmutableList() },
            { "Storage Path", new List<string[]>() { new[] { storagePath } }.ToImmutableList() },
            { "Verify Path", new List<string[]>() { new[] { verifyPath } }.ToImmutableList() },
            { "Paths to Skip", new List<string[]>() { new[] { pathsToSkip } }.ToImmutableList() },
            { "Verified", verified },
            { "Unverified", unverified },
            { "Moved", moved },
            { "Missing", missing },
            { "New", newFiles },
        };
    }
};