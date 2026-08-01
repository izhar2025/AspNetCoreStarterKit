// AspNetCoreStarterKit.Application/Features/Auth/RefreshToken.cs
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Application.Interfaces;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Auth;

public record RefreshTokenCommand : IRequest<ApiResponse<LoginResponseDto>>
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<LoginResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<ApiResponse<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _unitOfWork.Repository<RefreshToken>()
            .Query()
            .Include(rt => rt.User)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && !rt.IsRevoked, cancellationToken);

        if (refreshToken == null)
            return ApiResponse<LoginResponseDto>.Unauthorized("Invalid refresh token");

        if (refreshToken.IsExpired)
            return ApiResponse<LoginResponseDto>.Unauthorized("Refresh token expired");

        var user = refreshToken.User;
        if (user == null)
            return ApiResponse<LoginResponseDto>.Unauthorized("User not found");

        if (!user.IsActive)
            return ApiResponse<LoginResponseDto>.Unauthorized("User is inactive");

        // Revoke old token
        refreshToken.IsRevoked = true;
        _unitOfWork.Repository<RefreshToken>().Update(refreshToken);

        // Now user is guaranteed not null
        var roleName = user.Role?.RoleName ?? "User";

        var newAccessToken = _jwtService.GenerateAccessToken(user, roleName);
        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = _jwtService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = "Unknown"
        };

        await _unitOfWork.Repository<RefreshToken>().AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LoginResponseDto
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                RoleName = roleName
            }
        };

        return ApiResponse<LoginResponseDto>.Ok(response, "Token refreshed successfully");
    }
}

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}