// AspNetCoreStarterKit.Application/Features/Roles/DeleteRole.cs
using FluentValidation;
using MediatR;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Roles;

public record DeleteRoleCommand(int Id) : IRequest<ApiResponse<object>>;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, ApiResponse<object>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<object>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Repository<Role>().GetByIdAsync(request.Id, cancellationToken);

        if (role == null)
            return ApiResponse<object>.NotFound($"Role with ID {request.Id} not found");

        // Prevent deleting system roles
        if (role.IsSystemRole)
            return ApiResponse<object>.Fail($"Cannot delete system role '{role.RoleName}'");

        // Check if role has users
        var hasUsers = await _unitOfWork.Repository<User>()
            .AnyAsync(u => u.RoleId == request.Id && u.IsActive, cancellationToken);

        if (hasUsers)
            return ApiResponse<object>.Fail($"Cannot delete role '{role.RoleName}' because it has users assigned");

        // Soft delete
        role.IsActive = false;
        _unitOfWork.Repository<Role>().Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(string.Empty, "Role deleted successfully");
    }
}

public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}