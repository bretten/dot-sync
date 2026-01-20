namespace com.brettnamba.DotSync.Common.Application.Jobs.Contracts;

/// <summary>
/// Validates the job to ensure it can run
/// </summary>
public interface IJobValidator
{
    /// <summary>
    /// Makes sure the job is valid to run
    /// </summary>
    /// <param name="jobParameters">The job parameters to validate</param>
    /// <returns>True if the job can run</returns>
    Task<JobValidationResult> IsValid(IJobParameters jobParameters);
}

public sealed record JobValidationResult(bool IsValid, string Message);