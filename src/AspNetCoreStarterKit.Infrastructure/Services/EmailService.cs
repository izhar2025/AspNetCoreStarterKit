using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using AspNetCoreStarterKit.Application.Interfaces;

namespace AspNetCoreStarterKit.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpHost = _configuration["Email:SmtpHost"];
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUser = _configuration["Email:SmtpUser"];
        var smtpPass = _configuration["Email:SmtpPass"];
        var fromEmail = _configuration["Email:FromEmail"];

        using var client = new SmtpClient(smtpHost, smtpPort);
        client.Credentials = new NetworkCredential(smtpUser, smtpPass);
        client.EnableSsl = true;

        var message = new MailMessage(fromEmail, to, subject, body);
        message.IsBodyHtml = true;

        await client.SendMailAsync(message);
    }

    public async Task SendPasswordResetEmailAsync(string to, string fullName, string resetToken)
    {
        var resetBaseUrl = _configuration["App:PasswordResetUrl"] ?? "https://yourapp.com/reset-password";
        var resetLink = $"{resetBaseUrl}?email={Uri.EscapeDataString(to)}&token={Uri.EscapeDataString(resetToken)}";

        var subject = "Reset your password";
        var body = $@"
            <p>Hi {fullName},</p>
            <p>We received a request to reset your password. Click the link below to choose a new one:</p>
            <p><a href=""{resetLink}"">Reset your password</a></p>
            <p>This link expires in 24 hours. If you didn't request this, you can safely ignore this email.</p>";

        await SendEmailAsync(to, subject, body);
    }
}