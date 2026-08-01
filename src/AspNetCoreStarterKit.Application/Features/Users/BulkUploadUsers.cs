// AspNetCoreStarterKit.Application/Features/Users/BulkUploadUsers.cs
using FluentValidation;
using MediatR;
using System.Data;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Application.Interfaces;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Users;

public record BulkUploadUsersCommand : IRequest<ApiResponse<BulkUploadResult>>
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
}

public class BulkUploadUsersCommandHandler : IRequestHandler<BulkUploadUsersCommand, ApiResponse<BulkUploadResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBulkUploadService _bulkUploadService;
    private readonly IPasswordHasher _passwordHasher;

    public BulkUploadUsersCommandHandler(IUnitOfWork unitOfWork, IBulkUploadService bulkUploadService, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _bulkUploadService = bulkUploadService;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<BulkUploadResult>> Handle(BulkUploadUsersCommand request, CancellationToken cancellationToken)
    {
        var result = new BulkUploadResult();

        var data = await _bulkUploadService.ParseAsync<UserBulkUploadDto>(request.FileStream, result, cancellationToken);

        if (!data.Any())
            return ApiResponse<BulkUploadResult>.Fail("No valid rows found");

        // Get roles for lookup
        var roleNames = data.Select(d => d.RoleName).Distinct().ToList();
        var roles = await _unitOfWork.Repository<Role>()
            .FindAsync(r => roleNames.Contains(r.RoleName), cancellationToken);
        var roleDict = roles.ToDictionary(r => r.RoleName, r => r.Id);

        // Get existing users
        var existingUsers = await _unitOfWork.Repository<User>()
            .FindAsync(u => data.Select(d => d.Username).Contains(u.Username) ||
                           data.Select(d => d.Email).Contains(u.Email), cancellationToken);

        var existingUsernames = existingUsers.Select(u => u.Username).ToHashSet();
        var existingEmails = existingUsers.Select(u => u.Email).ToHashSet();

        var defaultPassword = "Temp@123";
        var defaultPasswordHash = _passwordHasher.HashPassword(defaultPassword);

        foreach (var item in data)
        {
            bool hasError = false;

            if (string.IsNullOrWhiteSpace(item.Username))
            {
                result.AddError(item.RowNumber, "Username", item.Username, "Username is required");
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(item.Email))
            {
                result.AddError(item.RowNumber, "Email", item.Email, "Email is required");
                hasError = true;
            }

            if (!roleDict.TryGetValue(item.RoleName, out int roleId))
            {
                result.AddError(item.RowNumber, "Role Name", item.RoleName, $"Role '{item.RoleName}' not found");
                hasError = true;
            }

            if (hasError)
            {
                result.Failed++;
                continue;
            }

            if (existingUsernames.Contains(item.Username))
            {
                result.AddError(item.RowNumber, "Username", item.Username, "Username already exists");
                result.Failed++;
                continue;
            }

            if (existingEmails.Contains(item.Email))
            {
                result.AddError(item.RowNumber, "Email", item.Email, "Email already exists");
                result.Failed++;
                continue;
            }

            var user = new User
            {
                Username = item.Username,
                Email = item.Email,
                FullName = item.FullName,
                PhoneNumber = item.PhoneNumber,
                RoleId = roleId,
                PasswordHash = defaultPasswordHash,
                MustChangePassword = item.MustChangePassword,
                PasswordExpiryDate = DateTime.Now.AddDays(90),
                IsActive = true
            };

            await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
            existingUsernames.Add(item.Username);
            existingEmails.Add(item.Email);
            result.Success++;
        }

        result.FinalizeResult();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        string message = result.Success > 0
            ? $"Upload complete: {result.Success} succeeded, {result.Failed} failed. Default password is '{defaultPassword}'"
            : "Upload failed: No valid records were imported";

        return ApiResponse<BulkUploadResult>.Ok(result, message);
    }
}

public class BulkUploadUsersCommandValidator : AbstractValidator<BulkUploadUsersCommand>
{
    public BulkUploadUsersCommandValidator()
    {
        RuleFor(x => x.FileStream).NotNull();
        RuleFor(x => x.FileName).NotEmpty()
            .Must(x => x.EndsWith(".xlsx") || x.EndsWith(".xls"))
            .WithMessage("Only Excel files are allowed");
    }
}