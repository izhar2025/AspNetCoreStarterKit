// AspNetCoreStarterKit.Domain/Entities/Security/User.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using AspNetCoreStarterKit.Domain.Common;

namespace AspNetCoreStarterKit.Domain.Entities.Security;

[Table("Users")]
public class User : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    public int RoleId { get; set; }

    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastPasswordChanged { get; set; }

    public bool IsLockedOut { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndDate { get; set; }

    public bool MustChangePassword { get; set; }
    public DateTime? PasswordExpiryDate { get; set; }

    // Access restrictions (for multi-tenant/location based access)
    public int? LocationId { get; set; }
    public int? ZoneId { get; set; }
    public int? GateId { get; set; }

    [ForeignKey("RoleId")]
    public virtual Role? Role { get; set; }

    public virtual ICollection<RefreshToken>? RefreshTokens { get; set; }
    public virtual ICollection<UserActivityLog>? ActivityLogs { get; set; }
}