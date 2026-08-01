// AspNetCoreStarterKit.Application/Interfaces/IArchiveService.cs
using System.Text.Json;
using AspNetCoreStarterKit.Domain.Common;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Interfaces;

public interface IArchiveService
{
    Task ArchiveAsync<T>(T entity, string reason, CancellationToken cancellationToken = default) where T : BaseEntity;
}

public class ArchiveService : IArchiveService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ArchiveService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task ArchiveAsync<T>(T entity, string reason, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        // Soft delete original
        entity.IsActive = false;
        _unitOfWork.Repository<T>().Update(entity);

        // TODO: Store archive in separate archive table
        // This would require creating archive tables per entity or using a generic approach

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}