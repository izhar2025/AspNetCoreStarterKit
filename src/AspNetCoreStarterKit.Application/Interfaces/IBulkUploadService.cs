using AspNetCoreStarterKit.Application.Common.Models;

namespace AspNetCoreStarterKit.Application.Interfaces;

public interface IBulkUploadService
{
    Task<List<TDto>> ParseAsync<TDto>(Stream stream, BulkUploadResult result, CancellationToken cancellationToken = default)
        where TDto : class, new();
}