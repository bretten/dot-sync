using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;

namespace DotSync.FileSystem.Application.Reporting;

/// <summary>
/// Defines a service that should generate a report out of a <see cref="StorageLocationIntegrityVerificationResult"/>
/// </summary>
public interface IIntegrityReporter
{
    /// <summary>
    /// Should output a report for <see cref="StorageLocationIntegrityVerificationResult"/>
    /// </summary>
    /// <param name="result">The result to generate a report for</param>
    /// <returns>The report</returns>
    Task<string> OutputDirectoryResult(StorageLocationIntegrityVerificationResult result);
}