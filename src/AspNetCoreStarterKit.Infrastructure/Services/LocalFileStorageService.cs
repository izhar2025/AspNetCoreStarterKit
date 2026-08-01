// AspNetCoreStarterKit.Infrastructure/Services/LocalFileStorageService.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using AspNetCoreStarterKit.Application.Interfaces;

namespace AspNetCoreStarterKit.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly string _basePath;

    public LocalFileStorageService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
        _basePath = configuration["FileStorage:LocalPath"] ?? Path.Combine(_environment.WebRootPath, "uploads");

        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadAsync(IFormFile file, string subFolder = "uploads", CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file provided");

        var folderPath = Path.Combine(_basePath, subFolder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(folderPath, uniqueFileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/uploads/{subFolder}/{uniqueFileName}";
    }

    public async Task<string> UploadBase64Async(string base64String, string fileName, string subFolder = "uploads", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(base64String))
            throw new ArgumentException("No file data provided");

        var base64Parts = base64String.Split(',');
        var fileData = Convert.FromBase64String(base64Parts.Length > 1 ? base64Parts[1] : base64Parts[0]);

        var folderPath = Path.Combine(_basePath, subFolder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(folderPath, uniqueFileName);

        await File.WriteAllBytesAsync(filePath, fileData, cancellationToken);

        return $"/uploads/{subFolder}/{uniqueFileName}";
    }

    public async Task<byte[]?> DownloadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

        if (!File.Exists(fullPath))
            return null;

        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }

    public Task<bool> DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

        if (!File.Exists(fullPath))
            return Task.FromResult(false);

        File.Delete(fullPath);
        return Task.FromResult(true);
    }

    public string GetFileUrl(string filePath)
    {
        return filePath;
    }

    public string GetFileSizeString(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}