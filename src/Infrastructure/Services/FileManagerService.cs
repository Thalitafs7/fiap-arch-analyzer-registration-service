using Amazon.S3;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class FileManagerService : IFileManagerService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public FileManagerService(IAmazonS3 amazonS3, IConfiguration configuration)
    {
        _s3Client = amazonS3;
        _bucketName = configuration["AWS:S3:BucketName"] ?? throw new InvalidOperationException("AWS:S3:BucketName not configured.");
    }

    public Task<int> CountFileByPrefix(string prefix)
    {
        throw new NotImplementedException();
    }

    public async Task UploadAsync(string fileKey, byte[] fileBytes)
    {
        await _s3Client.Upload(_bucketName, fileBytes, fileKey);
    }
}
