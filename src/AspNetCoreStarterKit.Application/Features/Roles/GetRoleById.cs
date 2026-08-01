// AspNetCoreStarterKit.Application/Features/Roles/GetRoleById.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Roles;

public record GetRoleByIdQuery(int Id) : IRequest<ApiResponse<RoleDto>>;

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, ApiResponse<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRoleByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Repository<Role>()
            .Query()
            .Include(r => r.RolePermissions!)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role == null)
            return ApiResponse<RoleDto>.NotFound($"Role with ID {request.Id} not found");

        var dto = new RoleDto
        {
            Id = role.Id,
            RoleName = role.RoleName,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            IsActive = role.IsActive,
            CreatedOn = role.CreatedOn,
            Permissions = role.RolePermissions?
                .Where(rp => rp.Permission != null)
                .Select(rp => new PermissionDto
                {
                    Id = rp.Permission!.Id,
                    PermissionName = rp.Permission.PermissionName,
                    Description = rp.Permission.Description,
                    Category = rp.Permission.Category,
                    Module = rp.Permission.Module
                }).ToList() ?? new()
        };

        // Get user count
        var userCount = await _unitOfWork.Repository<User>()
            .CountAsync(u => u.RoleId == role.Id && u.IsActive, cancellationToken);
        dto.UsersCount = userCount;

        return ApiResponse<RoleDto>.Ok(dto);
    }
}