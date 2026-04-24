using Amazon.S3;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class FileManagerService : IFileManagerService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAmazonS3 _s3Client;

    public FileManagerService(IHttpContextAccessor httpContextAccessor, IAmazonS3 amazonS3)
    {
        _httpContextAccessor = httpContextAccessor;
        _s3Client = amazonS3;
    }

    public Task<int> CountFileByPrefix(string prefix)
    {
        throw new NotImplementedException();
    }

    public async Task UploadAsync(string fileKey, byte[] fileBytes)
    {
        await _s3Client.Upload("BuckerName", fileBytes, fileKey);
    }
}
