using com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Tests.FileIntegrity.Services;

public class Sha256FileChecksumGeneratorTests
{
    private const string TestFilesPath = "FileIntegrity/Services/TestFiles/";

    [Fact]
    public void GenerateChecksum_File_ReturnsChecksum()
    {
        // Arrange
        var fileInfo = new FileInfo($"{TestFilesPath}checksum_file.txt");
        var generator = new Sha256FileChecksumGenerator();

        // Act
        var actual = generator.GenerateChecksum(fileInfo);

        // Assert
        Assert.Equal("gc8+ZmyAj0xclM2CVqRQdCDaFRyRXeyxncPGnZqlERM=", actual);
    }
}