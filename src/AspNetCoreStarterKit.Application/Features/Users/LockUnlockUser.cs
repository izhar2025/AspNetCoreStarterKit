// AspNetCoreStarterKit.Application/Features/Users/LockUnlockUser.cs
using FluentValidation;
using MediatR;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.Interfaces;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Users;

public record LockUserCommand(int Id) : IRequest<ApiResponse<object>>;
public record UnlockUserCommand(int Id) : IRequest<ApiResponse<object>>;
public record AdminResetPasswordCommand : IRequest<ApiResponse<object>>
{
    public int Id { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}

public class LockUserCommandHandler : IRequestHandler<LockUserCommand, ApiResponse<object>>
{
    private readonly IUnitOfWork _unitOfWork;

    public LockUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<object>> Handle(LockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
            return ApiResponse<object>.NotFound($"User with ID {request.Id} not found");

        user.IsLockedOut = true;
        user.LockoutEndDate = DateTime.Now.AddDays(30);
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(string.Empty, $"User '{user.Username}' has been locked");
    }
}

public class UnlockUserCommandHandler : IRequestHandler<UnlockUserCommand, ApiResponse<object>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UnlockUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<object>> Handle(UnlockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
            return ApiResponse<object>.NotFound($"User with ID {request.Id} not found");

        user.IsLockedOut = false;
        user.LockoutEndDate = null;
        user.FailedLoginAttempts = 0;
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(string.Empty, $"User '{user.Username}' has been unlocked");
    }
}

public class AdminResetPasswordCommandHandler : IRequestHandler<AdminResetPasswordCommand, ApiResponse<object>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public AdminResetPasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<object>> Handle(AdminResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
            return ApiResponse<object>.NotFound($"User with ID {request.Id} not found");

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.MustChangePassword = true;
        user.PasswordExpiryDate = DateTime.Now.AddDays(90);
        user.FailedLoginAttempts = 0;
        user.IsLockedOut = false;
        user.LockoutEndDate = null;

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(string.Empty, $"Password for user '{user.Username}' has been reset");
    }
}

public class LockUserCommandValidator : AbstractValidator<LockUserCommand>
{
    public LockUserCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class UnlockUserCommandValidator : AbstractValidator<UnlockUserCommand>
{
    public UnlockUserCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class AdminResetPasswordCommandValidator : AbstractValidator<AdminResetPasswordCommand>
{
    public AdminResetPasswordCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
    }
}