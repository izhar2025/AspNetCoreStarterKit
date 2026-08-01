// AspNetCoreStarterKit.Domain/Entities/Security/RolePermission.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AspNetCoreStarterKit.Domain.Common;

namespace AspNetCoreStarterKit.Domain.Entities.Security;

[Table("RolePermissions")]
public class RolePermission : BaseEntity
{
    [Required]
    public int RoleId { get; set; }

    [Required]
    public int PermissionId { get; set; }

    public DateTime GrantedAt { get; set; } = DateTime.Now;

    [ForeignKey("RoleId")]
    public virtual Role? Role { get; set; }

    [ForeignKey("PermissionId")]
    public virtual Permission? Permission { get; set; }
}