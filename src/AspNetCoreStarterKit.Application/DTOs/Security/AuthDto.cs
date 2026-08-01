// AspNetCoreStarterKit.Application/DTOs/Security/AuthDto.cs - Add RefreshToken
using AspNetCoreStarterKit.Application.DTOs.Security;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;  // ← ADD THIS
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = new();
}