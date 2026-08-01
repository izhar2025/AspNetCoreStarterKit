// AspNetCoreStarterKit.Application/Features/Roles/CreateRole.cs
using AutoMapper;
using FluentValidation;
using MediatR;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Roles;

public record CreateRoleCommand : IRequest<ApiResponse<RoleDto>>
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, ApiResponse<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateRoleCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // Check if role exists
        var exists = await _unitOfWork.Repository<Role>()
            .AnyAsync(r => r.RoleName == request.RoleName, cancellationToken);

        if (exists)
            return ApiResponse<RoleDto>.Fail($"Role '{request.RoleName}' already exists");

        var role = _mapper.Map<Role>(request);
        role.IsSystemRole = false;

        await _unitOfWork.Repository<Role>().AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Assign permissions
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
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var dto = _mapper.Map<RoleDto>(role);
        return ApiResponse<RoleDto>.Created(dto, "Role created successfully");
    }
}

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}