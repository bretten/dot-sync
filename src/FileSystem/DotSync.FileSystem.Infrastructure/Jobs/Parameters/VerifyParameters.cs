using com.brettnamba.DotSync.FileSystem.Application.Jobs;
using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Enums;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Jobs.Parameters;

public sealed record VerifyParameters(
    StorageLocationType StorageType,
    string? StoragePath,
    string? VerifyPath,
    string? PathsToSkip) : IJobParameters;