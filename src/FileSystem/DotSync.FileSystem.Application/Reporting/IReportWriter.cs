namespace com.brettnamba.DotSync.FileSystem.Application.Reporting;

public interface IReportWriter
{
    Task WriteReport(string report);
}