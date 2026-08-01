// AspNetCoreStarterKit.Domain/Entities/Security/Role.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AspNetCoreStarterKit.Domain.Common;

namespace AspNetCoreStarterKit.Domain.Entities.Security;

[Table("Roles")]
public class Role : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string RoleName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsSystemRole { get; set; } = false;

    // Navigation properties
    public virtual ICollection<User>? Users { get; set; }
    public virtual ICollection<RolePermission>? RolePermissions { get; set; }
}