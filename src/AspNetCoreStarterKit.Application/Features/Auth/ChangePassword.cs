// AspNetCoreStarterKit.Application/Features/Auth/ChangePassword.cs
using FluentValidation;
using MediatR;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.Interfaces;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Auth;

public record ChangePasswordCommand : IRequest<ApiResponse<object>>
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ApiResponse<object>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<object>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            return ApiResponse<object>.Unauthorized("User not authenticated");

        if (request.NewPassword != request.ConfirmPassword)
            return ApiResponse<object>.Fail("New password and confirmation do not match");

        var user = await _unitOfWork.Repository<User>().GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user == null)
            return ApiResponse<object>.NotFound("User not found");

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            return ApiResponse<object>.Fail("Current password is incorrect");

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.MustChangePassword = false;
        user.PasswordExpiryDate = DateTime.Now.AddDays(90);

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(string.Empty, "Password changed successfully");
    }
}

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
        RuleFor(x => x.ConfirmPassword).NotEmpty().Equal(x => x.NewPassword);
    }
}