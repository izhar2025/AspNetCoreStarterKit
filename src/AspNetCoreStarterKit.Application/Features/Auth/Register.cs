// AspNetCoreStarterKit.Application/Features/Auth/Register.cs
using FluentValidation;
using MediatR;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Application.Interfaces;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Auth;

public record RegisterCommand : IRequest<ApiResponse<LoginResponseDto>>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<LoginResponseDto>>
{
    // Self-registered users get the least-privileged role by default.
    // An admin can promote them afterwards via the Users/Roles endpoints.
    private const string DefaultRoleName = "Viewer";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public RegisterCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<ApiResponse<LoginResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var usernameExists = await _unitOfWork.Repository<User>()
            .AnyAsync(u => u.Username == request.Username, cancellationToken);
        if (usernameExists)
            return ApiResponse<LoginResponseDto>.Fail($"Username '{request.Username}' is already taken");

        var emailExists = await _unitOfWork.Repository<User>()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (emailExists)
            return ApiResponse<LoginResponseDto>.Fail($"Email '{request.Email}' is already registered");

        var defaultRole = await _unitOfWork.Repository<Role>()
            .FirstOrDefaultAsync(r => r.RoleName == DefaultRoleName, cancellationToken);
        if (defaultRole == null)
            return ApiResponse<LoginResponseDto>.Fail("Registration is not available right now. Please contact an administrator.");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            RoleId = defaultRole.Id,
            MustChangePassword = false,
            PasswordExpiryDate = DateTime.Now.AddDays(90),
            IsActive = true
        };

        await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auto-login: issue tokens immediately so the client can go straight into the app.
        var accessToken = _jwtService.GenerateAccessToken(user, defaultRole.RoleName);
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
                RoleName = defaultRole.RoleName,
                LastLoginAt = user.LastLoginAt,
                IsLockedOut = user.IsLockedOut,
                MustChangePassword = user.MustChangePassword,
                IsActive = user.IsActive
            }
        };

        return ApiResponse<LoginResponseDto>.Created(response, "Registration successful");
    }
}

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.ConfirmPassword).NotEmpty().Equal(x => x.Password).WithMessage("Passwords do not match");
    }
}
