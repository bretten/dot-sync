using com.brettnamba.DotSync.FileSystem.Application.Jobs;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;

public sealed record SortParameters(string? SourcePath, string? DestinationPath)
    : IJobParameters
{
    public string JobId => "Sort";
}