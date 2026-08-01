// AspNetCoreStarterKit.Domain/Entities/Security/PasswordResetToken.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AspNetCoreStarterKit.Domain.Common;

namespace AspNetCoreStarterKit.Domain.Entities.Security;

[Table("PasswordResetTokens")]
public class PasswordResetToken : BaseEntity
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(500)]
    public string Token { get; set; } = string.Empty;

    public DateTime Expiry { get; set; } = DateTime.Now.AddHours(24);

    public bool IsUsed { get; set; } = false;

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}