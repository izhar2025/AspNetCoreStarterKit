namespace AspNetCoreStarterKit.Domain.Common.Interfaces;

public interface ISoftDelete
{
    bool IsActive { get; set; }
}