// AspNetCoreStarterKit.Application/Features/Roles/UpdateRole.cs
using AutoMapper;
using FluentValidation;
using MediatR;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Roles;

public record UpdateRoleCommand : IRequest<ApiResponse<RoleDto>>
{
    public int Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, ApiResponse<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateRoleCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RoleDto>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Repository<Role>().GetByIdAsync(request.Id, cancellationToken);
        if (role == null)
            return ApiResponse<RoleDto>.NotFound($"Role with ID {request.Id} not found");

        // Check duplicate name
        var duplicateName = await _unitOfWork.Repository<Role>()
            .AnyAsync(r => r.RoleName == request.RoleName && r.Id != request.Id, cancellationToken);

        if (duplicateName)
            return ApiResponse<RoleDto>.Fail($"Role '{request.RoleName}' already exists");

        // Prevent modifying system roles' name
        if (role.IsSystemRole && role.RoleName != request.RoleName)
            return ApiResponse<RoleDto>.Fail($"Cannot rename system role '{role.RoleName}'");

        // Update role
        role.RoleName = request.RoleName;
        role.Description = request.Description;
        _unitOfWork.Repository<Role>().Update(role);

        // Update permissions - remove old, add new
        var existingPermissions = await _unitOfWork.Repository<RolePermission>()
            .FindAsync(rp => rp.RoleId == role.Id, cancellationToken);

        foreach (var existing in existingPermissions)
        {
            _unitOfWork.Repository<RolePermission>().Remove(existing);
        }

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

        var dto = _mapper.Map<RoleDto>(role);
        return ApiResponse<RoleDto>.Ok(dto, "Role updated successfully");
    }
}

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.RoleName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}