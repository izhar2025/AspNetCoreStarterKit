// AspNetCoreStarterKit.Application/Features/Auth/ForgotPassword.cs
using FluentValidation;
using MediatR;
using System.Security.Cryptography;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.Interfaces;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Auth;

public record ForgotPasswordCommand : IRequest<ApiResponse<object>>
{
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ApiResponse<object>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<ApiResponse<object>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
            return ApiResponse<object>.Ok(string.Empty, "If an account exists, a reset link has been sent.");

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            Expiry = DateTime.Now.AddHours(24),
            IsUsed = false
        };

        // Deactivate old tokens
        var oldTokens = await _unitOfWork.Repository<PasswordResetToken>()
            .FindAsync(t => t.UserId == user.Id && !t.IsUsed && t.Expiry > DateTime.Now, cancellationToken);

        foreach (var oldToken in oldTokens)
        {
            oldToken.IsUsed = true;
            _unitOfWork.Repository<PasswordResetToken>().Update(oldToken);
        }

        await _unitOfWork.Repository<PasswordResetToken>().AddAsync(resetToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send email (implement email sending)
        // await _emailService.SendPasswordResetEmail(user.Email, user.FullName, token);

        return ApiResponse<object>.Ok(string.Empty, "If an account exists, a reset link has been sent.");
    }
}

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}