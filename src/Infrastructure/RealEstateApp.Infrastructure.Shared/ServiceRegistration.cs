using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Infrastructure.Shared.Services;

namespace RealEstateApp.Infrastructure.Shared
{
    public static class ServiceRegistration
    {
        public static void AddSharedInfrastructure(this IServiceCollection services, string webRootPath = "wwwroot")
        {
            services.AddHttpClient();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IFileStorageService>(provider => new FileStorageService(webRootPath));
            services.AddTransient<IFinancingService, FinancingService>();
            services.AddTransient<IPaymentService, PaymentService>();
            services.AddTransient<IWhatsAppService, WhatsAppService>();
        }
    }
}
