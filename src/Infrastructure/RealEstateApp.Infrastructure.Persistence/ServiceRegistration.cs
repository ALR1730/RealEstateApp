using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Domain.Settings;
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

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    if (isPostgres && !string.IsNullOrWhiteSpace(connectionString))
                    {
                        var formattedPgConn = ConvertPostgresUrlToConnectionString(connectionString);
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
                    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
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

        private static string ConvertPostgresUrlToConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) return connectionString;
            if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
                !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            {
                return connectionString;
            }

            try
            {
                var uri = new Uri(connectionString);
                var userInfo = uri.UserInfo.Split(':');
                var user = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
                var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
                var host = uri.Host;
                var port = uri.Port > 0 ? uri.Port : 5432;
                var db = uri.AbsolutePath.TrimStart('/');

                return $"Host={host};Port={port};Database={db};Username={user};Password={pass};SSL Mode=Require;Trust Server Certificate=true;";
            }
            catch
            {
                return connectionString;
            }
        }
    }
}
