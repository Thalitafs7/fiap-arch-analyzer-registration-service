namespace Application.Common.Interfaces;

public interface IFileManagerService
{
    Task UploadAsync(string fileKey, byte[] fileBytes);
    Task<int> CountFileByPrefix(string prefix);
}
