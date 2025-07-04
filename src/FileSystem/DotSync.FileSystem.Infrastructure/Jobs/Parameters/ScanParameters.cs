using com.brettnamba.DotSync.FileSystem.Application.Jobs;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;

public sealed record ScanParameters(
    string? Path) : IJobParameters;