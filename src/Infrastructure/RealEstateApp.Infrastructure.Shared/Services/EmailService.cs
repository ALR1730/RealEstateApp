using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Infrastructure.Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration? _configuration;

        public EmailService() { }

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendAsync(string to, string subject, string body)
        {
            var host = _configuration?["Smtp:Host"];
            var portStr = _configuration?["Smtp:Port"];
            var username = _configuration?["Smtp:Username"];
            var password = _configuration?["Smtp:Password"];
            var from = _configuration?["Smtp:From"];

            var smtpConfigured = !string.IsNullOrWhiteSpace(host)
                                 && !string.IsNullOrWhiteSpace(portStr)
                                 && !string.IsNullOrWhiteSpace(username)
                                 && !string.IsNullOrWhiteSpace(password)
                                 && !string.IsNullOrWhiteSpace(from);

            if (!smtpConfigured)
            {
                Console.WriteLine($"[EmailService-Dev] To: {to} | Subject: {subject} | Body: {body}");
                return Task.CompletedTask;
            }

            if (!int.TryParse(portStr, out var port))
            {
                Console.WriteLine($"[EmailService-Dev] Puerto SMTP inválido '{portStr}', usando consola. To: {to} | Subject: {subject}");
                return Task.CompletedTask;
            }

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true,
            };

            var message = new MailMessage(from!, to, subject, body);
            client.Send(message);
            return Task.CompletedTask;
        }
    }
}
