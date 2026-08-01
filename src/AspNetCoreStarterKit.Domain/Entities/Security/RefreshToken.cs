// AspNetCoreStarterKit.Domain/Entities/Security/RefreshToken.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AspNetCoreStarterKit.Domain.Common;

namespace AspNetCoreStarterKit.Domain.Entities.Security;

[Table("RefreshTokens")]
public class RefreshToken : BaseEntity
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(500)]
    public string Token { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    [StringLength(50)]
    public string? RevokedByIp { get; set; }

    [StringLength(500)]
    public string? RevokedReason { get; set; }

    [StringLength(50)]
    public string? CreatedByIp { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    [NotMapped]
    public bool IsValid => !IsRevoked && !IsExpired;
}