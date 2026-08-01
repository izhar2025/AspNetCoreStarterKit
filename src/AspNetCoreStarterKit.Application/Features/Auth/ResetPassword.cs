// AspNetCoreStarterKit.Application/Features/Auth/ResetPassword.cs
using FluentValidation;
using MediatR;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.Interfaces;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Auth;

public record ResetPasswordCommand : IRequest<ApiResponse<object>>
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ApiResponse<object>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<object>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return ApiResponse<object>.Fail("Passwords do not match");

        var user = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
            return ApiResponse<object>.NotFound("User not found");

        var resetToken = await _unitOfWork.Repository<PasswordResetToken>()
            .FirstOrDefaultAsync(t => t.UserId == user.Id && t.Token == request.Token && !t.IsUsed && t.Expiry > DateTime.Now, cancellationToken);

        if (resetToken == null)
            return ApiResponse<object>.Fail("Invalid or expired reset token");

        resetToken.IsUsed = true;
        _unitOfWork.Repository<PasswordResetToken>().Update(resetToken);

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.MustChangePassword = false;
        user.PasswordExpiryDate = DateTime.Now.AddDays(90);
        user.FailedLoginAttempts = 0;
        user.IsLockedOut = false;
        user.LockoutEndDate = null;

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(string.Empty, "Password reset successfully");
    }
}

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
        RuleFor(x => x.ConfirmPassword).NotEmpty().Equal(x => x.NewPassword);
    }
}