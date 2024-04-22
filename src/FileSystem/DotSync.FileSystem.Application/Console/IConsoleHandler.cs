namespace DotSync.FileSystem.Application.Console;

/// <summary>
/// Defines a console handler
/// </summary>
public interface IConsoleHandler
{
    /// <summary>
    /// Should handle console command line arguments
    /// </summary>
    /// <param name="args">The command line arguments</param>
    Task Handle(string[] args);
}