// AspNetCoreStarterKit.Domain/Common/ArchivedEntity.cs
namespace AspNetCoreStarterKit.Domain.Common;

public abstract class ArchivedEntity
{
    public int Id { get; set; }
    public DateTime ArchivedOn { get; set; }
    public string? ArchivedBy { get; set; }
    public string? ArchiveReason { get; set; }
    public string OriginalData { get; set; } = string.Empty;
}