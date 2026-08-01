// AspNetCoreStarterKit.Application/Features/Roles/GetAllPermissions.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Roles;

public class GetAllPermissionsQuery : PaginationParams, IRequest<PagedResult<PermissionDto>>
{
    public string? Category { get; set; }
    public string? Module { get; set; }
}

public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, PagedResult<PermissionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllPermissionsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<PermissionDto>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Permission>().Query();

        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(p => p.Category == request.Category);

        if (!string.IsNullOrWhiteSpace(request.Module))
            query = query.Where(p => p.Module == request.Module);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(p => p.PermissionName.Contains(request.SearchTerm) ||
                                     (p.Description != null && p.Description.Contains(request.SearchTerm)));

        query = request.SortBy?.ToLower() switch
        {
            "permissionname" => request.SortDescending ? query.OrderByDescending(p => p.PermissionName) : query.OrderBy(p => p.PermissionName),
            "category" => request.SortDescending ? query.OrderByDescending(p => p.Category) : query.OrderBy(p => p.Category),
            "module" => request.SortDescending ? query.OrderByDescending(p => p.Module) : query.OrderBy(p => p.Module),
            _ => query.OrderBy(p => p.Module).ThenBy(p => p.PermissionName)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                PermissionName = p.PermissionName,
                Description = p.Description,
                Category = p.Category,
                Module = p.Module,
                IsActive = p.IsActive
            })
            .ToListAsync(cancellationToken);

        return PagedResult<PermissionDto>.Create(items, totalCount, request.PageNumber, request.PageSize);
    }
}