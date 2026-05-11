using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace Infrastructure.Services;

public static class S3Extension
{


    public static async Task<AbortMultipartUploadResponse> AbortMultipartUploadAsync(this IAmazonS3 s3Client, string bucketName, string key, string uploadId)
    {
        var request = new AbortMultipartUploadRequest
        {
            BucketName = bucketName,
            Key = key,
            UploadId = uploadId
        };
        return await s3Client.AbortMultipartUploadAsync(request);
    }


    public static async Task<byte[]> Dowload(this IAmazonS3 s3Client, string bucketName, string fileName)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = fileName
        };
        using var response = await s3Client.GetObjectAsync(request);
        using var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }


    public static async Task Upload(this IAmazonS3 s3Client, string bucketName, byte[] file, string fileName)
    {

        using TransferUtility transferUtility = new TransferUtility(s3Client);
        using MemoryStream ms = new MemoryStream(file);
        await transferUtility.UploadAsync(ms, bucketName, fileName);
    }
}