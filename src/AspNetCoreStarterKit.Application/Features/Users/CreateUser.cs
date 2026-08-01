// AspNetCoreStarterKit.Application/Features/Users/CreateUser.cs
using AutoMapper;
using FluentValidation;
using MediatR;
using System.Data;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Application.Interfaces;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Users;

public record CreateUserCommand : IRequest<ApiResponse<UserDto>>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Password { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public bool MustChangePassword { get; set; } = true;
    public DateTime? PasswordExpiryDate { get; set; }
    public int? LocationId { get; set; }
    public int? ZoneId { get; set; }
    public int? GateId { get; set; }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ApiResponse<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if username exists
        var usernameExists = await _unitOfWork.Repository<User>()
            .AnyAsync(u => u.Username == request.Username, cancellationToken);

        if (usernameExists)
            return ApiResponse<UserDto>.Fail($"Username '{request.Username}' already exists");

        // Check if email exists
        var emailExists = await _unitOfWork.Repository<User>()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
            return ApiResponse<UserDto>.Fail($"Email '{request.Email}' already exists");

        // Check if role exists
        var role = await _unitOfWork.Repository<Role>().GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
            return ApiResponse<UserDto>.Fail($"Role with ID {request.RoleId} not found");

        var user = _mapper.Map<User>(request);
        user.PasswordHash = _passwordHasher.HashPassword(request.Password);
        user.PasswordExpiryDate = request.PasswordExpiryDate ?? DateTime.Now.AddDays(90);

        await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<UserDto>(user);
        dto.RoleName = role.RoleName;

        return ApiResponse<UserDto>.Created(dto, "User created successfully");
    }
}

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.RoleId).GreaterThan(0);
    }
}