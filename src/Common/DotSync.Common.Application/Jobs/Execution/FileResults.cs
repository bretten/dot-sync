namespace com.brettnamba.DotSync.Common.Application.Jobs.Execution;

public sealed class FileResults
{
    public FileResults(IReadOnlyDictionary<string, IEnumerable<string[]>> fileResultLists)
    {
        FileResultLists = fileResultLists;
    }

    public IReadOnlyDictionary<string, IEnumerable<string[]>> FileResultLists { get; }
}