using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Mappings;
using RealEstateApp.Core.Application.Services;

namespace RealEstateApp.Core.Application
{
    public static class ServiceRegistration
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(Assembly.GetExecutingAssembly());
            });

            #region Application Services

            services.AddTransient<IPropertyService, PropertyService>();
            services.AddTransient<IPropertyTypeService, PropertyTypeService>();
            services.AddTransient<ISaleTypeService, SaleTypeService>();
            services.AddTransient<IImprovementService, ImprovementService>();
            services.AddTransient<IOfferService, OfferService>();
            services.AddTransient<IChatService, ChatService>();
            services.AddTransient<IFavoriteService, FavoriteService>();
            services.AddTransient<IAgentService, AgentService>();
            services.AddTransient<IUserActivityService, UserActivityService>();
            services.AddTransient<IAppointmentService, AppointmentService>();
<<<<<<< HEAD
=======
            services.AddTransient<IAgentVerificationService, AgentVerificationService>();
            services.AddTransient<ISubscriptionService, SubscriptionService>();
            services.AddTransient<ICurrencyService, CurrencyService>();
            services.AddTransient<ISavedSearchService, SavedSearchService>();
            services.AddTransient<IProvinceService, ProvinceService>();
            services.AddTransient<IPropertyValuationService, PropertyValuationService>();
            services.AddTransient<ILeadPipelineService, LeadPipelineService>();
            services.AddTransient<IBuyAbilityService, BuyAbilityService>();
            services.AddTransient<IReviewService, ReviewService>();
            services.AddTransient<ICommissionService, CommissionService>();
            services.AddTransient<IPropertyDocumentService, PropertyDocumentService>();
            services.AddTransient<IAiSearchService, AiSearchService>();
>>>>>>> feature/filtros-catalogo

            #endregion
        }
    }
}
