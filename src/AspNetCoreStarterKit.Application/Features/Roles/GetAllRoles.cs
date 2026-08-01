// AspNetCoreStarterKit.Application/Features/Roles/GetAllRoles.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Roles;

public class GetAllRolesQuery : PaginationParams, IRequest<PagedResult<RoleDto>>
{
    public bool? IsSystemRole { get; set; }
    public bool? IncludePermissions { get; set; } = true;
}

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, PagedResult<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllRolesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Role>().Query();

        if (request.IsSystemRole.HasValue)
            query = query.Where(r => r.IsSystemRole == request.IsSystemRole.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(r => r.RoleName.Contains(request.SearchTerm) ||
                                     (r.Description != null && r.Description.Contains(request.SearchTerm)));

        query = request.SortBy?.ToLower() switch
        {
            "rolename" => request.SortDescending ? query.OrderByDescending(r => r.RoleName) : query.OrderBy(r => r.RoleName),
            "createdon" => request.SortDescending ? query.OrderByDescending(r => r.CreatedOn) : query.OrderBy(r => r.CreatedOn),
            _ => query.OrderBy(r => r.RoleName)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                RoleName = r.RoleName,
                Description = r.Description,
                IsSystemRole = r.IsSystemRole,
                IsActive = r.IsActive,
                CreatedOn = r.CreatedOn
            })
            .ToListAsync(cancellationToken);

        // Get user counts
        foreach (var item in items)
        {
            var userCount = await _unitOfWork.Repository<User>()
                .CountAsync(u => u.RoleId == item.Id && u.IsActive, cancellationToken);
            item.UsersCount = userCount;

            if (request.IncludePermissions == true)
            {
                var permissions = await _unitOfWork.Repository<RolePermission>()
                    .Query()
                    .Include(rp => rp.Permission)
                    .Where(rp => rp.RoleId == item.Id)
                    .Select(rp => new PermissionDto
                    {
                        Id = rp.Permission!.Id,
                        PermissionName = rp.Permission.PermissionName,
                        Description = rp.Permission.Description,
                        Category = rp.Permission.Category,
                        Module = rp.Permission.Module
                    })
                    .ToListAsync(cancellationToken);

                item.Permissions = permissions;
            }
        }

        return PagedResult<RoleDto>.Create(items, totalCount, request.PageNumber, request.PageSize);
    }
}