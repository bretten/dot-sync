using System.Text;
using com.brettnamba.DotSync.Common.Extensions;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace com.brettnamba.DotSync.FileSystem.Application.Reporting;

/// <summary>
/// Generates a HTML report out of a <see cref="FileSetIntegrityVerificationResult"/>
/// </summary>
public sealed class HtmlIntegrityReporter : IIntegrityReporter
{
    /// <summary>
    /// Outputs a report for <see cref="FileSetIntegrityVerificationResult"/>
    /// </summary>
    /// <param name="storageLocation">Where the result was stored</param>
    /// <param name="result">The result to generate a report for</param>
    /// <returns>The report</returns>
    public Task<string> OutputFileSetResult(StorageLocation storageLocation, FileSetIntegrityVerificationResult result)
    {
        var b = new StringBuilder();
        b.AppendLine("<html>");
        b.AppendLine("<body>");
        b.AppendLine("<style>");
        b.AppendLine("table {border-spacing: 30px;}");
        b.AppendLine("th, td {padding-top: 5px; padding-bottom: 5   px; padding-left: 5px; padding-right: 5px; }");
        b.AppendLine("</style>");
        b.AppendLine("<h1>Results</h1>");
        b.AppendLine($"<strong>Storage Location Type:</strong> {storageLocation.Type.GetDisplayName()}");
        b.AppendLine("<br/>");
        b.AppendLine($"<strong>Storage Location Path:</strong> {storageLocation.Path.Value}");
        b.AppendLine("<h2>Overview</h2>");

        b.AppendLine("</table>");

        b.AppendLine("</body>");
        b.AppendLine("</html>");

        return Task.FromResult(b.ToString());
    }

    /// <summary>
    /// Converts byte count to a human readable format
    /// </summary>
    private static string FormatBytes(long bytes)
    {
        string[] suffix = ["B", "KB", "MB", "GB", "TB"];
        int i;
        double dblSByte = bytes;
        for (i = 0; i < suffix.Length && bytes >= 1024; i++, bytes /= 1024)
        {
            dblSByte = bytes / 1024.0;
        }

        return $"{dblSByte:0.##} {suffix[i]}";
    }
}