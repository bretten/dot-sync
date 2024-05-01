using System.Text.RegularExpressions;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.Services;

/// <summary>
/// <inheritdoc cref="IFileMetadataReader"/>
/// <para>Reads metadata from a Windows file</para>
/// </summary>
public sealed class WindowsFileMetadataReader : IFileMetadataReader
{
    /// <summary>
    /// The shell application used to access metadata
    /// </summary>
    private readonly dynamic _shellApp;

    /// <summary>
    /// Constructor
    /// </summary>
    public WindowsFileMetadataReader()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new NotWindowsException();
        }

        var shellAppType = Type.GetTypeFromProgID("Shell.Application");
        _shellApp = Activator.CreateInstance(shellAppType!) ?? throw new WindowsShellCouldNotBeCreatedException();
    }

    /// <summary>
    /// <inheritdoc cref="IFileMetadataReader.ReadPhotoOrVideoTakenDate"/>
    /// <para>Attempts to read the taken date from different meta data properties. If it cannot find a date, it will throw an exception of type <see cref="DateTakenCouldNotBeFoundException"/></para>
    /// </summary>
    public DateTime ReadPhotoOrVideoTakenDate(FileSystemPath path)
    {
        var shellObject = GetShellObject(new FileInfo(path.Value));
        try
        {
            return DateTime.Parse(GetMetadataValue(shellObject, DateTakenId));
        }
        catch (FormatException)
        {
        }

        try
        {
            return DateTime.Parse(GetMetadataValue(shellObject, MediaCreatedId));
        }
        catch (FormatException)
        {
        }

        try
        {
            return DateTime.Parse(GetMetadataValue(shellObject, DateModifiedId));
        }
        catch (FormatException)
        {
        }

        throw new DateTakenCouldNotBeFoundException($"Date taken could not be found for {path.Value}");
    }

    /// <summary>
    /// Gets the shell object for the specified file
    /// </summary>
    private ShellDirectoryAndFile GetShellObject(FileInfo fileInfo)
    {
        var shellDir = _shellApp.NameSpace(fileInfo.Directory?.FullName);
        if (shellDir == null)
            throw new WindowsShellObjectCouldNotBeCreatedException(
                $"Could not create directory object for: {fileInfo.Directory?.FullName}");
        var shellFile = shellDir.ParseName(fileInfo.Name);
        if (shellDir == null)
            throw new WindowsShellObjectCouldNotBeCreatedException(
                $"Could not create file object for: {fileInfo.Name}");
        return new ShellDirectoryAndFile(Directory: shellDir, File: shellFile);
    }

    /// <summary>
    /// Reads metadata from the specified shell object for the specified ID
    /// </summary>
    private static string GetMetadataValue(ShellDirectoryAndFile shellObject, int id)
    {
        string str = shellObject.Directory.GetDetailsOf(shellObject.File, id).ToString();

        return Regex.Replace(str, @"[^\x20-\x7F]", ""); // replace '[^ -~\t]'
    }

    private sealed record ShellDirectoryAndFile(dynamic Directory, dynamic File);

    private sealed class NotWindowsException(
        string? message = $"{nameof(WindowsFileMetadataReader)} cannot be used on a non-Windows environment")
        : Exception(message);

    private sealed class WindowsShellCouldNotBeCreatedException(
        string? message = "The Windows shell could not be created") : Exception(message);

    private sealed class WindowsShellObjectCouldNotBeCreatedException(string? message) : Exception(message);

    private sealed class DateTakenCouldNotBeFoundException(string? message) : Exception(message);

    private const int DateTakenId = 12;
    private const int MediaCreatedId = 208;
    private const int DateModifiedId = 3;
}