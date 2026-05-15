using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class LocalFileManagerService : IFileManagerService
{
    private readonly string _basePath;

    public LocalFileManagerService(IConfiguration configuration)
    {
        _basePath = configuration["LocalStorage:BasePath"]
            ?? throw new InvalidOperationException("LocalStorage:BasePath não configurado.");
    }

    public async Task UploadAsync(string fileKey, byte[] fileBytes)
    {
        var fullPath = Path.Combine(_basePath, fileKey);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, fileBytes);
    }

    public Task<int> CountFileByPrefix(string prefix)
    {
        var dir = Path.Combine(_basePath, prefix);
        if (!Directory.Exists(dir)) return Task.FromResult(0);
        return Task.FromResult(Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length);
    }
}
