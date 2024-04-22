using System.Text;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;

namespace DotSync.FileSystem.Application.Reporting;

/// <summary>
/// Generates a HTML report out of a <see cref="StorageLocationIntegrityVerificationResult"/>
/// </summary>
public sealed class HtmlIntegrityReporter : IIntegrityReporter
{
    /// <summary>
    /// Outputs a report for <see cref="StorageLocationIntegrityVerificationResult"/>
    /// </summary>
    /// <param name="result">The result to generate a report for</param>
    /// <returns>The report</returns>
    public Task<string> OutputDirectoryResult(StorageLocationIntegrityVerificationResult result)
    {
        var b = new StringBuilder();
        b.AppendLine("<html>");
        b.AppendLine("<body>");
        b.AppendLine("<style>");
        b.AppendLine("table {border-spacing: 30px;}");
        b.AppendLine("th, td {padding-top: 5px; padding-bottom: 5   px; padding-left: 5px; padding-right: 5px; }");
        b.AppendLine("</style>");
        b.AppendLine("<h1>Results</h1>");
        b.AppendLine("<h2>Overview</h2>");
        b.AppendLine($"""
                                  <ul>
                                      <li>Total files: {result.FileCount}</li>
                                      <li>Verified files: {result.SuccessfulVerifications}</li>
                                      <li>Unverified files: {result.UnverifiedFiles.Count}</li>
                                      <li>Files no longer in storage location: {result.FilesNoLongerInStorageLocation.Count}</li>
                                      <li>Total size (bytes): {result.TotalSize}</li>
                                      <li>Total size: {FormatBytes(result.TotalSize)}</li>
                                  </ul>
                      """);

        b.AppendLine("<h2>Unverified Files</h2>");
        b.AppendLine("<table>");
        b.AppendLine("<tr><th>Path</th><th>Checksum</th></tr>");
        foreach (var file in result.UnverifiedFiles)
        {
            b.AppendLine($"<tr><td>{file.Path.Value}</td><td>{file.Checksum.Value}</td></tr>");
        }

        b.AppendLine("</table>");

        b.AppendLine("<h2>Files No Longer In Storage Location</h2>");
        b.AppendLine("<table>");
        b.AppendLine("<tr><th>Path</th><th>Checksum</th></tr>");
        foreach (var file in result.FilesNoLongerInStorageLocation)
        {
            b.AppendLine($"<tr><td>{file.Path.Value}</td><td>{file.Sha256Checksum.Value}</td></tr>");
        }

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