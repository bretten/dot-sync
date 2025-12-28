using System.Collections.Immutable;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace com.brettnamba.DotSync.FileSystem.Application.Orchestration;

/// <summary>
/// Verification result of <see cref="IStorageLocationIntegrityVerificationService"/>
/// </summary>
public readonly record struct StorageLocationIntegrityVerificationResult(
    StorageLocation StorageLocation,
    FileSetIntegrityVerificationResult Result,
    string Report)
{
    public IReadOnlyList<FileResultList> GenerateReport(string storageType, string storagePath,
        string verifyPath, string pathsToSkip, DateTime startTime)
    {
        var verified = new List<string[]>() { new string[] { "Verified Count", Result.TotalVerified.ToString() } };
        var unverified = Result.Unverified.Select(x => (string[])[x.Path.Value, x.Checksum.Value, x.Size.ToString()])
            .ToImmutableList();
        var moved = Result.Moved.Select(x => (string[])[x.Path.Value]).ToImmutableList();
        var missing = Result.Missing.Select(x => (string[])[x.Path.Value]).ToImmutableList();
        var newFiles = Result.New.Select(x => (string[])[x.Path.Value]).ToImmutableList();

        var duration = DateTime.UtcNow.Subtract(startTime);

        return new List<FileResultList>()
        {
            new("Storage Type", new List<string[]>() { new[] { storageType } }.ToImmutableList(), false),
            new("Storage Path", new List<string[]>() { new[] { storagePath } }.ToImmutableList(), false),
            new("Verify Path", new List<string[]>() { new[] { verifyPath } }.ToImmutableList(), false),
            new("Paths to Skip", new List<string[]>() { new[] { pathsToSkip } }.ToImmutableList(), false),
            new("Duration", new List<string[]>() { new[] { $"{duration.TotalMinutes} mins" } }.ToImmutableList(),
                false),
            new("Verified", verified, false),
            new("Unverified", unverified, true),
            new("Moved", moved, true),
            new("Missing", missing, true),
            new("New", newFiles, true)
        }.AsReadOnly();
    }
}

public record FileResultList(string Name, IReadOnlyList<string[]> FileList, bool ShowFileCount);