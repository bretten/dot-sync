namespace com.brettnamba.DotSync.FileSystem.Application.Configuration;

/// <summary>
/// Configurable options for directories
/// </summary>
/// <param name="SortDir">The directory to sort from</param>
public sealed record DirectoryConfiguration(string SortDir)
{
    public DirectoryConfiguration() : this("ToSort")
    {
    }

    public const string Section = "Directories";
};