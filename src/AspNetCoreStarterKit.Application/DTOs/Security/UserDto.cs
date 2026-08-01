// AspNetCoreStarterKit.Application/DTOs/Security/UserDto.cs
using AspNetCoreStarterKit.Application.Common.Attributes;

namespace AspNetCoreStarterKit.Application.DTOs.Security;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsLockedOut { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTime? PasswordExpiryDate { get; set; }
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public int? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public int? GateId { get; set; }
    public string? GateName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
}

public class CreateUserDto
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

public class UpdateUserDto : CreateUserDto
{
    public int Id { get; set; }
}

public class UserBulkUploadDto
{
    public int RowNumber { get; set; }

    [ExcelColumn("Username", order: 0, IsRequired = true, Example = "john.doe")]
    public string Username { get; set; } = string.Empty;

    [ExcelColumn("Email", order: 1, IsRequired = true, Example = "john@example.com")]
    public string Email { get; set; } = string.Empty;

    [ExcelColumn("Full Name", order: 2, IsRequired = true, Example = "John Doe")]
    public string FullName { get; set; } = string.Empty;

    [ExcelColumn("Phone Number", order: 3, IsRequired = false, Example = "+1234567890")]
    public string? PhoneNumber { get; set; }

    [ExcelColumn("Role Name", order: 4, IsRequired = true, Example = "Admin")]
    public string RoleName { get; set; } = string.Empty;

    [ExcelColumn("Must Change Password", order: 5, IsRequired = false, Example = "TRUE")]
    public bool MustChangePassword { get; set; } = true;
}