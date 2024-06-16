namespace com.brettnamba.DotSync.FileSystem.Domain.FileStorage.ValueObjects;

public record struct FileSha256Checksum
{
    public string Value { get; }

    private FileSha256Checksum(string hash)
    {
        Value = hash;
    }

    public static FileSha256Checksum Create(string hash)
    {
        return new FileSha256Checksum(hash);
    }
}