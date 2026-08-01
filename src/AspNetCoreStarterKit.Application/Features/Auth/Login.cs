// AspNetCoreStarterKit.Application/Features/Auth/Login.cs
using FluentValidation;
using MediatR;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Application.Interfaces;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Auth;

public record LoginCommand : IRequest<ApiResponse<LoginResponseDto>>
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<ApiResponse<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Username, cancellationToken);

        if (user == null)
            return ApiResponse<LoginResponseDto>.Unauthorized("Invalid username or password");

        if (!user.IsActive)
            return ApiResponse<LoginResponseDto>.Unauthorized("Account is deactivated");

        if (user.IsLockedOut && user.LockoutEndDate > DateTime.Now)
            return ApiResponse<LoginResponseDto>.Unauthorized($"Account locked until {user.LockoutEndDate}");

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.IsLockedOut = true;
                user.LockoutEndDate = DateTime.Now.AddMinutes(15);
            }
            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ApiResponse<LoginResponseDto>.Unauthorized("Invalid username or password");
        }

        user.FailedLoginAttempts = 0;
        user.IsLockedOut = false;
        user.LockoutEndDate = null;
        user.LastLoginAt = DateTime.Now;
        _unitOfWork.Repository<User>().Update(user);

        var role = await _unitOfWork.Repository<Role>().GetByIdAsync(user.RoleId, cancellationToken);
        var roleName = role?.RoleName ?? "User";

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user, roleName);
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = _jwtService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = "Unknown"
        };
        await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LoginResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleId,
                RoleName = roleName,
                LastLoginAt = user.LastLoginAt,
                IsLockedOut = user.IsLockedOut,
                MustChangePassword = user.MustChangePassword,
                IsActive = user.IsActive
            }
        };

        return ApiResponse<LoginResponseDto>.Ok(response, "Login successful");
    }
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}