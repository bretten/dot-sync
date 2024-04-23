namespace com.brettnamba.DotSync.FileSystem.Domain.Tests.TestClasses;

public static class PathHelper
{
    public static string AsPath(this string path)
    {
        return path.Replace('\u005c', Path.AltDirectorySeparatorChar);
    }
}