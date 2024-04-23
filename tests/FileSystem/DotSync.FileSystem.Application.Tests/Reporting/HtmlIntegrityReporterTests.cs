using System.Text.RegularExpressions;
using com.brettnamba.DotSync.FileSystem.Application.Reporting;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.FileIntegrity.ValueObjects.TestClasses;

namespace com.brettnamba.DotSync.FileSystem.Application.Tests.Reporting;

public class HtmlIntegrityReporterTests
{
    [Fact]
    public async Task OutputDirectoryResult_MixedResults_ReturnsReport()
    {
        // Arrange
        var fileResult1 =
            Faker.FakeFileIntegrityVerificationResult(path: "dir1/file1.txt", checksum: "file1", isVerified: true,
                size: 1);
        var fileResult2 =
            Faker.FakeFileIntegrityVerificationResult(path: "dir1/dir2/file2.txt", checksum: "file2", isVerified: true,
                size: 2);
        var fileResult3 =
            Faker.FakeFileIntegrityVerificationResult(path: "dir3/file3.txt", checksum: "file3", isVerified: true,
                size: 3);
        var fileResult4 =
            Faker.FakeFileIntegrityVerificationResult(path: "dir4/file4.txt", checksum: "file4", isVerified: false,
                size: 4);
        var fileResult5 =
            Faker.FakeFileIntegrityVerificationResult(path: "dir5/file5.txt", checksum: "file5", isVerified: false,
                size: 5);

        var fileFromPreviousRun1 = Domain.Tests.Files.TestClasses.Faker.FakeFile(
            path: "dir4/file4.txt", checksum: "file4", size: 4);
        var fileFromPreviousRun2 = Domain.Tests.Files.TestClasses.Faker.FakeFile(
            path: "dir5/file5.txt", checksum: "file5", size: 5);
        var fileFromPreviousRun3 = Domain.Tests.Files.TestClasses.Faker.FakeFile(
            path: "dir6/file6.txt", checksum: "file6", size: 6);
        var fileFromPreviousRun4 = Domain.Tests.Files.TestClasses.Faker.FakeFile(
            path: "dir6/file7.txt", checksum: "file7", size: 7);

        var storageLocationResult = Faker.FakeFileSetIntegrityVerificationResult(
            new List<FileIntegrityVerificationResult>()
                { fileResult1, fileResult2, fileResult3, fileResult4, fileResult5 },
            new List<DotFile>
                { fileFromPreviousRun1, fileFromPreviousRun2, fileFromPreviousRun3, fileFromPreviousRun4 });

        var reporter = new HtmlIntegrityReporter();

        // Act
        var actual = await reporter.OutputDirectoryResult(storageLocationResult);

        // Assert
        Assert.Equal(Regex.Replace("""
                                   <html>
                                   <body>
                                   <style>
                                   table {border-spacing: 30px;}
                                   th, td {padding-top: 5px; padding-bottom: 5px; padding-left: 5px; padding-right: 5px; }
                                   </style>
                                   <h1>Results</h1>
                                   <h2>Overview</h2>
                                               <ul>
                                                   <li>Total files: 5</li>
                                                   <li>Verified files: 3</li>
                                                   <li>Unverified files: 2</li>
                                                   <li>Files no longer in set: 2</li>
                                                   <li>Total size (bytes): 15</li>
                                                   <li>Total size: 15 B</li>
                                               </ul>
                                   <h2>Unverified Files</h2>
                                   <table>
                                   <tr><th>Path</th><th>Checksum</th></tr>
                                   <tr><td>dir4\file4.txt</td><td>file4</td></tr>
                                   <tr><td>dir5\file5.txt</td><td>file5</td></tr>
                                   </table>
                                   <h2>Files no longer in set</h2>
                                   <table>
                                   <tr><th>Path</th><th>Checksum</th></tr>
                                   <tr><td>dir6\file6.txt</td><td>file6</td></tr>
                                   <tr><td>dir6\file7.txt</td><td>file7</td></tr>
                                   </table>
                                   </body>
                                   </html>
                                   """, @"\s", ""), Regex.Replace(actual, @"\s", ""));
    }
}