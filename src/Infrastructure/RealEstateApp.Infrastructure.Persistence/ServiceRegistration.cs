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

            if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("ApplicationDb"));
            }
            else
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? configuration["DATABASE_URL"]
                    ?? configuration["DATABASE_URL_INTERNAL"];

                var isPostgres = configuration.GetValue<bool>("UsePostgreSQL") ||
                    (!string.IsNullOrWhiteSpace(connectionString) &&
                     (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                      connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
                      connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
                      (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) && connectionString.Contains("Port=5432", StringComparison.OrdinalIgnoreCase))));

                var trustServerCertificate = configuration.GetValue<bool>("Database:TrustServerCertificate") ||
                    string.Equals(configuration["DB_TRUST_SERVER_CERTIFICATE"], "true", StringComparison.OrdinalIgnoreCase);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    if (isPostgres && !string.IsNullOrWhiteSpace(connectionString))
                    {
                        var formattedPgConn = PostgresConnectionStringParser.Parse(connectionString, trustServerCertificate);
                        options.UseNpgsql(formattedPgConn, m =>
                        {
                            m.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                        });
                    }
                    else
                    {
                        options.UseSqlServer(
                            connectionString,
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
