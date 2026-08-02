// AspNetCoreStarterKit.Application/Features/Auth/ForgotPassword.cs
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
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

        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, token);
        }
        catch (Exception ex)
        {
            // Don't fail the request or reveal delivery status to the caller -
            // that would leak whether the account exists. Log for ops instead.
            _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
        }

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