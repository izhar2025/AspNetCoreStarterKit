// AspNetCoreStarterKit.Domain/Entities/Security/LoginAttempt.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AspNetCoreStarterKit.Domain.Common;

namespace AspNetCoreStarterKit.Domain.Entities.Security;

[Table("LoginAttempts")]
public class LoginAttempt : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(45)]
    public string IpAddress { get; set; } = string.Empty;

    public DateTime AttemptedAt { get; set; } = DateTime.Now;

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = string.Empty;

    [StringLength(500)]
    public string? FailureReason { get; set; }

    [StringLength(512)]
    public string? UserAgent { get; set; }
}