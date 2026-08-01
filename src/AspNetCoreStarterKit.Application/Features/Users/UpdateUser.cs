// AspNetCoreStarterKit.Application/Features/Users/UpdateUser.cs
using AutoMapper;
using FluentValidation;
using MediatR;
using System.Data;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Users;

public record UpdateUserCommand : IRequest<ApiResponse<UserDto>>
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int RoleId { get; set; }
    public bool MustChangePassword { get; set; } = true;
    public DateTime? PasswordExpiryDate { get; set; }
    public int? LocationId { get; set; }
    public int? ZoneId { get; set; }
    public int? GateId { get; set; }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ApiResponse<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
            return ApiResponse<UserDto>.NotFound($"User with ID {request.Id} not found");

        // Check duplicate username (excluding self)
        var duplicateUsername = await _unitOfWork.Repository<User>()
            .AnyAsync(u => u.Username == request.Username && u.Id != request.Id, cancellationToken);

        if (duplicateUsername)
            return ApiResponse<UserDto>.Fail($"Username '{request.Username}' already exists");

        // Check duplicate email (excluding self)
        var duplicateEmail = await _unitOfWork.Repository<User>()
            .AnyAsync(u => u.Email == request.Email && u.Id != request.Id, cancellationToken);

        if (duplicateEmail)
            return ApiResponse<UserDto>.Fail($"Email '{request.Email}' already exists");

        // Check if role exists
        var role = await _unitOfWork.Repository<Role>().GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
            return ApiResponse<UserDto>.Fail($"Role with ID {request.RoleId} not found");

        // Don't update password here - use separate reset password endpoint
        user.Username = request.Username;
        user.Email = request.Email;
        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.RoleId = request.RoleId;
        user.MustChangePassword = request.MustChangePassword;
        user.PasswordExpiryDate = request.PasswordExpiryDate;
        user.LocationId = request.LocationId;
        user.ZoneId = request.ZoneId;
        user.GateId = request.GateId;

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<UserDto>(user);
        dto.RoleName = role.RoleName;

        return ApiResponse<UserDto>.Ok(dto, "User updated successfully");
    }
}

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RoleId).GreaterThan(0);
    }
}