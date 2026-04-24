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




    //public static void AddS3(this IServiceCollection services, IConfiguration configuration)
    //{
    //    var s3Config = configuration.GetSection("AWS:S3");
    //    var accessKey = s3Config["AccessKey"];
    //    var secretKey = s3Config["SecretKey"];
    //    var region = s3Config["Region"];
    //    if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(region))
    //    {
    //        throw new InvalidOperationException("AWS S3 configuration is missing or incomplete.");
    //    }
    //    var credentials = new BasicAWSCredentials(accessKey, secretKey);
    //    var s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.GetBySystemName(region));
    //    services.AddSingleton<IAmazonS3>(s3Client);

    //}
}