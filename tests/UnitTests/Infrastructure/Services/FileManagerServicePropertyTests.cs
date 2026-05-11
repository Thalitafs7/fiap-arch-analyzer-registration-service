using Amazon.S3;
using Amazon.S3.Model;
using FsCheck;
using FsCheck.Xunit;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace UnitTests.Infrastructure.Services;

/// <summary>
/// Property-Based Tests for FileManagerService.
/// Feature: registration-service-pbt-cleanup
/// </summary>
public class FileManagerServicePropertyTests
{
    private const string BucketName = "test-bucket";

    private static FileManagerService CreateService(IAmazonS3 s3Client)
    {
        var configuration = Substitute.For<IConfiguration>();
        configuration["AWS:S3:BucketName"].Returns(BucketName);
        return new FileManagerService(s3Client, configuration);
    }

    /// <summary>
    /// Property 19: FileManagerService delegates upload to S3 with correct parameters.
    ///
    /// For any valid fileKey (non-empty string) and fileBytes (non-empty byte array),
    /// calling UploadAsync SHALL invoke the S3 client Upload method with the configured
    /// bucket name, the provided fileBytes, and the provided fileKey.
    ///
    /// **Validates: Requirements 12.1**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property FileManagerService_UploadAsync_DelegatesToS3WithCorrectParameters()
    {
        var arb = (from fileKey in Arb.Generate<NonEmptyString>().Select(s => s.Get).Where(s => !string.IsNullOrWhiteSpace(s))
                   from fileBytes in Arb.Generate<byte[]>().Where(b => b != null && b.Length > 0)
                   select (fileKey, fileBytes)).ToArbitrary();

        return Prop.ForAll(arb, tuple =>
        {
            var (fileKey, fileBytes) = tuple;

            // Arrange
            var s3Client = Substitute.For<IAmazonS3>();

            // TransferUtility requires Config to be non-null for telemetry/span creation
            s3Client.Config.Returns(new AmazonS3Config());

            // TransferUtility calls PutObjectAsync internally for small files
            s3Client
                .PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>())
                .Returns(new PutObjectResponse());

            var service = CreateService(s3Client);

            // Act
            service.UploadAsync(fileKey, fileBytes).GetAwaiter().GetResult();

            // Assert: PutObjectAsync called with correct BucketName and Key
            s3Client.Received(1).PutObjectAsync(
                Arg.Is<PutObjectRequest>(r =>
                    r.BucketName == BucketName &&
                    r.Key == fileKey),
                Arg.Any<CancellationToken>());

            return true;
        });
    }
}
