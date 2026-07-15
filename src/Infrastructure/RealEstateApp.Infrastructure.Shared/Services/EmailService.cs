using System;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Infrastructure.Shared.Services
{
    public class EmailService : IEmailService
    {
        public Task SendAsync(string to, string subject, string body)
        {
            // Simulación de envío de correo electrónico
            Console.WriteLine($"Enviando correo a {to} con el asunto: '{subject}'");
            return Task.CompletedTask;
        }
    }
}
