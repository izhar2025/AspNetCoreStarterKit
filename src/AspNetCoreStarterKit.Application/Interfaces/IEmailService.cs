namespace AspNetCoreStarterKit.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);

    Task SendPasswordResetEmailAsync(string to, string fullName, string resetToken);
}