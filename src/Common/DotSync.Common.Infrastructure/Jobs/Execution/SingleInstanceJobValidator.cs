using com.brettnamba.DotSync.Common.Application.Jobs;
using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;

namespace com.brettnamba.DotSync.Common.Infrastructure.Jobs.Execution;

/// <summary>
/// Job validator that ensures only a single instance of a job will run
///
/// Good for a single-user system with lower memory
/// </summary>
public sealed class SingleInstanceJobValidator : IJobValidator
{
    /// <summary>
    /// Used to check active jobs
    /// </summary>
    private readonly IJobManager _jobManager;

    public SingleInstanceJobValidator(IJobManager jobManager)
    {
        _jobManager = jobManager;
    }

    /// <inheritdoc/>
    public Task<JobValidationResult> IsValid(IJobParameters jobParameters)
    {
        var activeState = (JobState state) => state == JobState.Queued || state == JobState.InProgress;
        var sameJobType = (JobType x, JobType y) => x == y;
        var activeSimilarJobs = _jobManager.Jobs
            .Where(x => activeState(x.State) && sameJobType(x.Type, jobParameters.Type))
            .ToList();

        var isValid = activeSimilarJobs.Count == 0;
        return Task.FromResult(new JobValidationResult(isValid,
            isValid ? string.Empty : "Active job already exists"
        ));
    }
}