// AspNetCoreStarterKit.Application/Features/Users/GetAllUsers.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Users;

public class GetAllUsersQuery : PaginationParams, IRequest<PagedResult<UserDto>>
{
    public int? RoleId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsLockedOut { get; set; }
}

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResult<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllUsersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<User>().Query();

        if (request.RoleId.HasValue && request.RoleId.Value > 0)
            query = query.Where(u => u.RoleId == request.RoleId.Value);

        if (request.IsActive.HasValue)
            query = query.Where(u => u.IsActive == request.IsActive.Value);

        if (request.IsLockedOut.HasValue && request.IsLockedOut.Value)
            query = query.Where(u => u.IsLockedOut && u.LockoutEndDate > DateTime.Now);

        // Use request.SearchTerm from base class
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(u => u.Username.Contains(request.SearchTerm) ||
                                     u.FullName.Contains(request.SearchTerm) ||
                                     u.Email.Contains(request.SearchTerm) ||
                                     (u.PhoneNumber != null && u.PhoneNumber.Contains(request.SearchTerm)));
        }

        query = query.Include(u => u.Role);

        query = request.SortBy?.ToLower() switch
        {
            "username" => request.SortDescending ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
            "fullname" => request.SortDescending ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName),
            "email" => request.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "rolename" => request.SortDescending
                ? query.OrderByDescending(u => u.Role != null ? u.Role.RoleName : null)
                : query.OrderBy(u => u.Role != null ? u.Role.RoleName : null),
            "lastloginat" => request.SortDescending ? query.OrderByDescending(u => u.LastLoginAt) : query.OrderBy(u => u.LastLoginAt),
            "createdon" => request.SortDescending ? query.OrderByDescending(u => u.CreatedOn) : query.OrderBy(u => u.CreatedOn),
            _ => query.OrderBy(u => u.Username)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                RoleId = u.RoleId,
                RoleName = u.Role != null ? u.Role.RoleName : null,
                LastLoginAt = u.LastLoginAt,
                IsLockedOut = u.IsLockedOut,
                MustChangePassword = u.MustChangePassword,
                PasswordExpiryDate = u.PasswordExpiryDate,
                IsActive = u.IsActive,
                CreatedOn = u.CreatedOn,
                CreatedBy = u.CreatedBy
            })
            .ToListAsync(cancellationToken);

        return PagedResult<UserDto>.Create(items, totalCount, request.PageNumber, request.PageSize);
    }
}