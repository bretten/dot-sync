using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Domain.FileIntegrity.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Repositories;
using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.ValueObjects;
using com.brettnamba.DotSync.FileSystem.Domain.Tests.Files.TestClasses;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services;
using com.brettnamba.DotSync.FileSystem.Infrastructure.FileIntegrity.Services.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Tests.FileIntegrity.Services;

public class AmazonS3FileIntegrityVerifierTests
{
    [Fact]
    public async Task Verify_PaginatorReturnsNonOkStatusCode_ThrowsException()
    {
        // Arrange
        var responses = new List<ListObjectsV2Response>()
        {
            new()
            {
                HttpStatusCode = HttpStatusCode.OK
            },
            new()
            {
                HttpStatusCode = HttpStatusCode.NotFound
            }
        };
        var stubS3 = MockS3ListObjectsV2Paginator(responses);

        var verifier = new AmazonS3FileIntegrityVerifier(Mock.Of<IFileRepository>(), Mock.Of<IFileChecksumGenerator>(),
            Mock.Of<ILogger<IFileIntegrityVerifier>>(), stubS3.Object, "bucket");

        var action = async () => await verifier.Verify(FileSystemPath.Create(""), Array.Empty<FileSystemPath>());

        // Act
        var actual = await Record.ExceptionAsync(action);

        // Assert
        Assert.NotNull(actual);
        Assert.IsType<AmazonS3ListObjectsPaginationException>(actual);
    }

    [Fact]
    public async Task Verify_S3ObjectWithNoChecksum_ThrowsException()
    {
        // Arrange
        const string bucketName = "bucket";
        const string key = "key1";
        var responses = new List<ListObjectsV2Response>()
        {
            new()
            {
                HttpStatusCode = HttpStatusCode.OK,
                S3Objects =
                [
                    new S3Object
                    {
                        Key = key
                    }
                ]
            }
        };
        var stubS3 = MockS3ListObjectsV2Paginator(responses);
        stubS3.Setup(x => x.GetObjectMetadataAsync(It.Is<GetObjectMetadataRequest>(
                y => y.Key == key && y.BucketName == bucketName), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetObjectMetadataResponse)null!);

        var verifier = new AmazonS3FileIntegrityVerifier(Mock.Of<IFileRepository>(), Mock.Of<IFileChecksumGenerator>(),
            Mock.Of<ILogger<IFileIntegrityVerifier>>(), stubS3.Object, bucketName);

        var action = async () =>
            await verifier.Verify(FileSystemPath.Create(bucketName), Array.Empty<FileSystemPath>());

        // Act
        var actual = await Record.ExceptionAsync(action);

        // Assert
        Assert.NotNull(actual);
        Assert.IsType<AmazonS3MissingChecksumException>(actual);
    }

    [Fact]
    public async Task Verify_VerifiedAndUnverifiedFile_ReturnsVerificationResult()
    {
        // Arrange
        const string bucketName = "bucket";
        const string verifiedKey = "verified";
        const string unverifiedKey = "unverified";
        const string skipKey = "skip/this/file";
        var responses = new List<ListObjectsV2Response>()
        {
            new()
            {
                HttpStatusCode = HttpStatusCode.OK,
                S3Objects =
                [
                    new S3Object
                    {
                        Key = verifiedKey
                    }
                ]
            },
            new()
            {
                HttpStatusCode = HttpStatusCode.OK,
                S3Objects =
                [
                    new S3Object
                    {
                        Key = unverifiedKey
                    }
                ]
            },
            new()
            {
                HttpStatusCode = HttpStatusCode.OK,
                S3Objects =
                [
                    new S3Object
                    {
                        Key = skipKey
                    }
                ]
            }
        };

        var verifiedMetadata = new GetObjectMetadataResponse()
        {
            ChecksumSHA256 = "verified256"
        };
        var unverifiedMetadata = new GetObjectMetadataResponse()
        {
            ChecksumSHA256 = "unverified256"
        };
        var verifiedChecksum = FileSha256Checksum.Create("verified256");
        var unverifiedChecksum = FileSha256Checksum.Create("unverified256");

        var stubS3 = MockS3ListObjectsV2Paginator(responses);
        stubS3.Setup(x => x.GetObjectMetadataAsync(It.Is<GetObjectMetadataRequest>(
                y => y.Key == verifiedKey && y.BucketName == bucketName), It.IsAny<CancellationToken>()))
            .ReturnsAsync(verifiedMetadata);
        stubS3.Setup(x => x.GetObjectMetadataAsync(It.Is<GetObjectMetadataRequest>(
                y => y.Key == unverifiedKey && y.BucketName == bucketName), It.IsAny<CancellationToken>()))
            .ReturnsAsync(unverifiedMetadata);

        var stubFileRepository = new Mock<IFileRepository>();
        stubFileRepository.Setup(x => x.GetFileByChecksum(verifiedChecksum))
            .ReturnsAsync(Faker.FakeFile(path: "verified", checksum: "verified256"));
        stubFileRepository.Setup(x => x.GetFileByChecksum(unverifiedChecksum))
            .ReturnsAsync((DotFile?)null);

        var verifier = new AmazonS3FileIntegrityVerifier(stubFileRepository.Object, Mock.Of<IFileChecksumGenerator>(),
            Mock.Of<ILogger<IFileIntegrityVerifier>>(), stubS3.Object, bucketName);

        // Act
        var actual = await verifier.Verify(FileSystemPath.Create(bucketName), new List<FileSystemPath>()
        {
            FileSystemPath.Create("skip")
        });

        // Assert
        Assert.Equal(2, actual.FileCount);
        Assert.Equal(1, actual.SuccessfulVerifications);
        Assert.Equal(1, actual.UnverifiedFiles.Count);
        Assert.Contains(
            FileIntegrityVerificationResult.Verified(FileSystemPath.Create("verified"), verifiedChecksum, 0),
            actual.Results);
        Assert.Contains(
            FileIntegrityVerificationResult.Unverified(FileSystemPath.Create("unverified"), unverifiedChecksum, 0),
            actual.Results);
        stubS3.Verify(
            x => x.GetObjectMetadataAsync(
                It.Is<GetObjectMetadataRequest>(y => y.Key == skipKey && y.BucketName == bucketName),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Mock<IAmazonS3> MockS3ListObjectsV2Paginator(IEnumerable<ListObjectsV2Response> responses)
    {
        // The queue will mock the enumerable behavior of the paginator
        var queue = new Queue<ListObjectsV2Response>(responses);

        // The current response from the paginator
        ListObjectsV2Response? current = null;

        // Mock the enumerator using the queue
        var stubEnumerator = new Mock<IAsyncEnumerator<ListObjectsV2Response>>();
        stubEnumerator.Setup(x => x.MoveNextAsync())
            .ReturnsAsync(() => queue.Count > 0)
            .Callback(() =>
            {
                // Calling MoveNext on the queue is the same as dequeuing
                if (queue.Count > 0)
                {
                    current = queue.Dequeue();
                }
            });
        stubEnumerator.Setup(x => x.Current)
            .Returns(() => current!);
        stubEnumerator.Setup(x => x.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        var stubEnumerable = new Mock<IPaginatedEnumerable<ListObjectsV2Response>>();
        stubEnumerable.Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(stubEnumerator.Object);

        var stubPaginator = new Mock<IListObjectsV2Paginator>();
        stubPaginator.Setup(x => x.Responses)
            .Returns(stubEnumerable.Object);

        var stubS3 = new Mock<IAmazonS3>();
        stubS3.Setup(x => x.Paginators.ListObjectsV2(It.IsAny<ListObjectsV2Request>()))
            .Returns(stubPaginator.Object);

        return stubS3;
    }
}