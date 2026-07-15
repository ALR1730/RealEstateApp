using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace RealEstateApp.Core.Application
{
    public static class ServiceRegistration
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            // Registrar validaciones, mediador o servicios de aplicación aquí
        }
    }
}
