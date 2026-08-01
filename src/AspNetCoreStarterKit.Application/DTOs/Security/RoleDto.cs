// AspNetCoreStarterKit.Application/DTOs/Security/RoleDto.cs
namespace AspNetCoreStarterKit.Application.DTOs.Security;

public class RoleDto
{
    public int Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public int UsersCount { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new();
}

public class CreateRoleDto
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}

public class UpdateRoleDto : CreateRoleDto
{
    public int Id { get; set; }
}

public class PermissionDto
{
    public int Id { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Module { get; set; }
    public bool IsActive { get; set; }
}

public class AssignPermissionsToRoleDto
{
    public int RoleId { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}