using com.brettnamba.DotSync.FileSystem.Application.Orchestration;

namespace com.brettnamba.DotSync.FileSystem.Application.Console;

/// <summary>
/// Console handler
/// </summary>
public class ConsoleHandler(IDirectoryVerificationService directoryVerificationService) : IConsoleHandler
{
    /// <summary>
    /// Handles console command line arguments
    /// </summary>
    /// <param name="args">The command line arguments</param>
    public async Task Handle(string[] args)
    {
        if (args.Length < 1)
        {
            throw new RequiredArgumentNotProvided("No arguments provided");
        }

        // Determine the type of command
        var command = args[0];

        switch (command.ToLower())
        {
            case "verify":
                await Verify(args);
                break;
            default:
                throw new UnknownCommandException($"Unknown command {command}");
        }
    }

    /// <summary>
    /// Verifies the integrity of a directory
    /// </summary>
    private async Task Verify(IReadOnlyList<string> args)
    {
        if (args.Count < 3)
        {
            throw new RequiredArgumentNotProvided(
                "Could not run verify. Need to include directory and output file arguments");
        }

        var result = await directoryVerificationService.Execute(args[1]);
        System.Console.WriteLine($"Total files: {result.Result.FileCount}");
        System.Console.WriteLine($"Verified files: {result.Result.SuccessfulVerifications}");
        System.Console.WriteLine($"Unverified files: {result.Result.UnverifiedFiles.Count}");
        System.Console.WriteLine(
            $"Files no longer in storage location: {result.Result.FilesNoLongerInStorageLocation.Count}");
        System.Console.WriteLine($"Total size (bytes): {result.Result.TotalSize}");
        await File.WriteAllTextAsync(args[2], result.Report);
    }

    private sealed class UnknownCommandException(string? message) : Exception;

    private sealed class RequiredArgumentNotProvided(string? message) : Exception;
}