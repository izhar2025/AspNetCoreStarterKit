// AspNetCoreStarterKit.Application/Features/Auth/GetCurrentUser.cs
using MediatR;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Application.Interfaces;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Auth;

public record GetCurrentUserQuery : IRequest<ApiResponse<UserDto>>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, ApiResponse<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            return ApiResponse<UserDto>.Unauthorized("User not authenticated");

        var user = await _unitOfWork.Repository<User>().GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);

        if (user == null)
            return ApiResponse<UserDto>.NotFound("User not found");

        var role = await _unitOfWork.Repository<Role>().GetByIdAsync(user.RoleId, cancellationToken);

        var dto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            RoleId = user.RoleId,
            RoleName = role?.RoleName,
            LastLoginAt = user.LastLoginAt,
            IsLockedOut = user.IsLockedOut,
            MustChangePassword = user.MustChangePassword,
            PasswordExpiryDate = user.PasswordExpiryDate,
            IsActive = user.IsActive,
            CreatedOn = user.CreatedOn,
            CreatedBy = user.CreatedBy
        };

        return ApiResponse<UserDto>.Ok(dto);
    }
}