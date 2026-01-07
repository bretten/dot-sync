namespace com.brettnamba.DotSync.FileSystem.Application.Jobs;

public sealed class FileResults
{
    public FileResults(IReadOnlyDictionary<string, IEnumerable<string[]>> fileResultLists)
    {
        FileResultLists = fileResultLists;
    }

    public IReadOnlyDictionary<string, IEnumerable<string[]>> FileResultLists { get; }
}