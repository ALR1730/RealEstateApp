using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Settings;
using RealEstateApp.Infrastructure.Persistence.Configuration;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;
using RealEstateApp.Infrastructure.Persistence.Services;

namespace RealEstateApp.Infrastructure.Persistence
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            #region DbContext

            services.AddHttpContextAccessor();

            var dbSettings = DatabaseConnectionResolver.Resolve(configuration);

            if (dbSettings.Provider == DatabaseProviderType.InMemory)
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("ApplicationDb"));
            }
            else
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    if (dbSettings.Provider == DatabaseProviderType.PostgreSql &&
                        !string.IsNullOrWhiteSpace(dbSettings.ConnectionString))
                    {
                        options.UseNpgsql(dbSettings.ConnectionString, m =>
                        {
                            m.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                        });
                    }
                    else
                    {
                        options.UseSqlServer(
                            dbSettings.ConnectionString,
                            m =>
                            {
                                m.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                                m.UseNetTopologySuite();
                            });
                    }
                });
            }

            #endregion

            #region Identity

            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                // Configuración de contraseña
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // Configuración de lockout
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // Configuración de usuario
                options.User.RequireUniqueEmail = true;

                // Requiere confirmación de email
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            #endregion

            #region Services

            services.Configure<JWTSettings>(configuration.GetSection("JWTSettings"));
            services.AddTransient<IAccountService, AccountService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            #endregion

            #region Repositories

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IPropertyRepository, PropertyRepository>();
            services.AddScoped<IPropertyTypeRepository, PropertyTypeRepository>();
            services.AddScoped<IOfferRepository, OfferRepository>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IFavoriteRepository, FavoriteRepository>();
            services.AddScoped<IPropertyImageRepository, PropertyImageRepository>();
            services.AddScoped<IUserActivityRepository, UserActivityRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<IProvinceRepository, ProvinceRepository>();
            services.AddScoped<IMunicipalityRepository, MunicipalityRepository>();
            services.AddScoped<IAgentVerificationRepository, AgentVerificationRepository>();
            services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
            services.AddScoped<IAgentSubscriptionRepository, AgentSubscriptionRepository>();
            services.AddScoped<IPropertyImprovementRepository, PropertyImprovementRepository>();
            services.AddScoped<ISavedSearchRepository, SavedSearchRepository>();
            services.AddScoped<IPropertyPriceHistoryRepository, PropertyPriceHistoryRepository>();
            services.AddScoped<IPropertyValuationRepository, PropertyValuationRepository>();
            services.AddScoped<ILeadPipelineRepository, LeadPipelineRepository>();
            services.AddScoped<IBuyAbilityEvaluationRepository, BuyAbilityEvaluationRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<ICommissionRepository, CommissionRepository>();
            services.AddScoped<IPropertyDocumentRepository, PropertyDocumentRepository>();

            #endregion
        }
    }
}
