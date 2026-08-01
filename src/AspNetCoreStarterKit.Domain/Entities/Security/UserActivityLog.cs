// AspNetCoreStarterKit.Domain/Entities/Security/UserActivityLog.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AspNetCoreStarterKit.Domain.Common;

namespace AspNetCoreStarterKit.Domain.Entities.Security;

[Table("UserActivityLogs")]
public class UserActivityLog : BaseEntity
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string ActivityType { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(45)]
    public string? IpAddress { get; set; }

    [StringLength(512)]
    public string? UserAgent { get; set; }

    public DateTime ActivityAt { get; set; } = DateTime.Now;

    [StringLength(50)]
    public string? Status { get; set; }

    [StringLength(500)]
    public string? Details { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}