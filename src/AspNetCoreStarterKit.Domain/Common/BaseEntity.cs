using System.ComponentModel.DataAnnotations;

namespace AspNetCoreStarterKit.Domain.Common;

public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }

    public string? TenantId { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsActive { get; set; } = true;
}