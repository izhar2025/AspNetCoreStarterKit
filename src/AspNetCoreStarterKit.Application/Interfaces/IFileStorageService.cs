// AspNetCoreStarterKit.Application/Interfaces/IFileStorageService.cs
using Microsoft.AspNetCore.Http;

namespace AspNetCoreStarterKit.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(IFormFile file, string subFolder = "uploads", CancellationToken cancellationToken = default);
    Task<string> UploadBase64Async(string base64String, string fileName, string subFolder = "uploads", CancellationToken cancellationToken = default);
    Task<byte[]?> DownloadAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string filePath, CancellationToken cancellationToken = default);
    string GetFileUrl(string filePath);
    string GetFileSizeString(long bytes);
}