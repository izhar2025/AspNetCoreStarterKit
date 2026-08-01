// AspNetCoreStarterKit.Application/Features/Roles/AssignPermissions.cs
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Roles;

public record AssignPermissionsToRoleCommand : IRequest<ApiResponse<RoleDto>>
{
    public int RoleId { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}

public class AssignPermissionsToRoleCommandHandler : IRequestHandler<AssignPermissionsToRoleCommand, ApiResponse<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public AssignPermissionsToRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<RoleDto>> Handle(AssignPermissionsToRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Repository<Role>().GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
            return ApiResponse<RoleDto>.NotFound($"Role with ID {request.RoleId} not found");

        // Remove existing permissions
        var existingPermissions = await _unitOfWork.Repository<RolePermission>()
            .FindAsync(rp => rp.RoleId == request.RoleId, cancellationToken);

        foreach (var existing in existingPermissions)
        {
            _unitOfWork.Repository<RolePermission>().Remove(existing);
        }

        // Add new permissions
        if (request.PermissionIds.Any())
        {
            var permissions = await _unitOfWork.Repository<Permission>()
                .FindAsync(p => request.PermissionIds.Contains(p.Id), cancellationToken);

            foreach (var permission in permissions)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id,
                    GrantedAt = DateTime.Now
                };
                await _unitOfWork.Repository<RolePermission>().AddAsync(rolePermission, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get updated role with permissions


        var updatedRole = await _unitOfWork.Repository<Role>()
            .Query()  // ← ADD THIS
            .Include(r => r.RolePermissions!)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == role.Id, cancellationToken);

        var dto = new RoleDto
        {
            Id = updatedRole!.Id,
            RoleName = updatedRole.RoleName,
            Description = updatedRole.Description,
            IsSystemRole = updatedRole.IsSystemRole,
            IsActive = updatedRole.IsActive,
            CreatedOn = updatedRole.CreatedOn,
            Permissions = updatedRole.RolePermissions?
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

        return ApiResponse<RoleDto>.Ok(dto, "Permissions assigned successfully");
    }
}

public class AssignPermissionsToRoleCommandValidator : AbstractValidator<AssignPermissionsToRoleCommand>
{
    public AssignPermissionsToRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).GreaterThan(0);
    }
}