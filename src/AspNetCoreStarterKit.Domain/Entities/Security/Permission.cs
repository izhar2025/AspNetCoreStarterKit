// AspNetCoreStarterKit.Domain/Entities/Security/Permission.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AspNetCoreStarterKit.Domain.Common;

namespace AspNetCoreStarterKit.Domain.Entities.Security;

[Table("Permissions")]
public class Permission : BaseEntity
{
    [Required]
    [StringLength(150)]
    public string PermissionName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [StringLength(50)]
    public string? Module { get; set; }

    // Navigation properties
    public virtual ICollection<RolePermission>? RolePermissions { get; set; }
}