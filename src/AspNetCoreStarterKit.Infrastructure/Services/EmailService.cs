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
}