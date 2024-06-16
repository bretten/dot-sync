using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Entities;

namespace com.brettnamba.DotSync.FileSystem.Application.Reporting;

/// <summary>
/// Defines a service that should generate a report out of a <see cref="FileSetIntegrityVerificationResult"/>
/// </summary>
public interface IIntegrityReporter
{
    /// <summary>
    /// Should output a report for <see cref="FileSetIntegrityVerificationResult"/>
    /// </summary>
    /// <param name="storageLocation">Where the result was stored</param>
    /// <param name="result">The result to generate a report for</param>
    /// <returns>The report</returns>
    Task<string> OutputFileSetResult(StorageLocation storageLocation, FileSetIntegrityVerificationResult result);
}