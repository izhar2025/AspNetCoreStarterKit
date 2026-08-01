// AspNetCoreStarterKit.Application/Features/Users/DeleteUser.cs
using FluentValidation;
using MediatR;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Users;

public record DeleteUserCommand(int Id) : IRequest<ApiResponse<object>>;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ApiResponse<object>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<object>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.Id, cancellationToken);

        if (user == null)
            return ApiResponse<object>.NotFound($"User with ID {request.Id} not found");

        // Soft delete
        user.IsActive = false;
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(string.Empty, "User deleted successfully");
    }
}

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}