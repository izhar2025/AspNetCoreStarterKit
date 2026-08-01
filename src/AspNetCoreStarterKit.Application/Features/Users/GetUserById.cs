// AspNetCoreStarterKit.Application/Features/Users/GetUserById.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Users;

public record GetUserByIdQuery(int Id) : IRequest<ApiResponse<UserDto>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ApiResponse<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>()
            .Query()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
            return ApiResponse<UserDto>.NotFound($"User with ID {request.Id} not found");

        var dto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            RoleId = user.RoleId,
            RoleName = user.Role?.RoleName,
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