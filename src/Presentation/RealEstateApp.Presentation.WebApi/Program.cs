using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RealEstateApp.Core.Application;
using RealEstateApp.Core.Domain.Settings;
using RealEstateApp.Infrastructure.Persistence;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Seeds;
using RealEstateApp.Infrastructure.Shared;
using RealEstateApp.Presentation.WebApi.Hubs;

// RealEstateApp WebApi V2 - .NET 10 + SignalR + JWT Bearer (Updated Mappings)
var builder = WebApplication.CreateBuilder(args);

// Soporte para puerto dinámico de Render/contenedores en la nube
var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(renderPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");
}

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
}).AddMvcOptions(o => o.Filters.Add<RealEstateApp.Presentation.WebApi.Filters.ApiGlobalExceptionFilter>());

// Cultura invariante para un parsing numérico estable (decimales con punto, fechas ISO)
var invariantCulture = CultureInfo.InvariantCulture;
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(invariantCulture, invariantCulture);
    options.SupportedCultures = new[] { invariantCulture };
    options.SupportedUICultures = new[] { invariantCulture };
    options.RequestCultureProviders.Clear();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();

// Configurar HSTS para la API REST en entorno de producción
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// Configurar Rate Limiting para la API REST
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.PermitLimit = 15;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

// Configurar política de CORS adaptativa (local + Render)
var allowedOriginsConfig = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
var frontendUrlEnv = Environment.GetEnvironmentVariable("FRONTEND_URL");
var defaultOrigins = new List<string>
{
    "http://localhost:5173", 
    "http://127.0.0.1:5173", 
    "http://localhost:3000", 
    "http://localhost:5174", 
    "http://localhost:5000", 
    "https://localhost:5001", 
    "http://localhost:5196", 
    "https://localhost:7196" 
};
if (allowedOriginsConfig != null) defaultOrigins.AddRange(allowedOriginsConfig);
if (!string.IsNullOrWhiteSpace(frontendUrlEnv)) defaultOrigins.Add(frontendUrlEnv.TrimEnd('/'));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if (defaultOrigins.Any(d => string.Equals(d, origin, StringComparison.OrdinalIgnoreCase))) return true;
            if (Uri.TryCreate(origin, UriKind.Absolute, out var uri) && 
                (uri.Host.EndsWith(".onrender.com", StringComparison.OrdinalIgnoreCase) || uri.Host == "localhost"))
            {
                return true;
            }
            return false;
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// Configurar Swagger/OpenAPI con autenticación Bearer Token
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "RealEstateApp API",
        Description = "API RESTful para la plataforma inmobiliaria RealEstateApp V2 protegida por JWT Bearer Token y conectada al Frontend React SPA.",
        Contact = new OpenApiContact
        {
            Name = "RealEstateApp Team",
            Email = "support@realestateapp.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT con el prefijo 'Bearer ', ejemplo: Bearer eyJhbGciOi..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Registrar capas de la arquitectura Onion
builder.Services.AddApplicationLayer();
builder.Services.AddPersistenceInfrastructure(builder.Configuration);
builder.Services.AddSharedInfrastructure();

var jwtKey = builder.Configuration["JWTSettings:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    jwtKey = "RealEstateAppSuperSecretKeyForDevelopmentAndTesting2026";
}

// Configuración de Autenticación por Tokens JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["JWTSettings:Issuer"] ?? "RealEstateAppIdentity",
        ValidAudience = builder.Configuration["JWTSettings:Audience"] ?? "RealEstateAppUser",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new { hasError = true, error = "No está autorizado para acceder a este recurso" });
            return context.Response.WriteAsync(result);
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new { hasError = true, error = "No tiene permisos suficientes para acceder a este recurso" });
            return context.Response.WriteAsync(result);
        }
    };
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

var app = builder.Build();

try
{
    // Ejecutar Seeds y Migraciones al iniciar la aplicación
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            if (dbContext.Database.IsRelational())
            {
                try
                {
                    await dbContext.Database.MigrateAsync();
                }
                catch (Exception migEx)
                {
                    Console.WriteLine($"Nota en MigrateAsync: {migEx.Message}. Aplicando EnsureCreatedAsync...");
                    await dbContext.Database.EnsureCreatedAsync();
                }
            }
            else
            {
                await dbContext.Database.EnsureCreatedAsync();
            }

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

            await DefaultRoles.SeedAsync(roleManager);

            // Seeding de usuarios de prueba y datos de demostración (en desarrollo o por variable de entorno)
            var shouldSeed = app.Environment.IsDevelopment() || 
                             builder.Configuration.GetValue<bool>("SeedInitialData") || 
                             string.Equals(Environment.GetEnvironmentVariable("SEED_INITIAL_DATA"), "true", StringComparison.OrdinalIgnoreCase);

            if (shouldSeed)
            {
                await DefaultAdminUser.SeedAsync(userManager);
                await DefaultAgentUser.SeedAsync(userManager);
                await DefaultClientUser.SeedAsync(userManager);
                await DefaultDeveloperUser.SeedAsync(userManager);
                await DefaultOwnerUser.SeedAsync(userManager);
                await DefaultSubscriptionPlans.SeedAsync(dbContext);
                await DefaultDominicanProvinces.SeedAsync(dbContext);
                await DefaultRealEstateData.SeedAsync(dbContext, userManager);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al ejecutar los seeds o inicializar base de datos: {ex.Message}");
        }
    }

    // Enable Swagger UI en desarrollo o si se habilita explícitamente en producción
    var enableSwagger = app.Environment.IsDevelopment() || 
                        builder.Configuration.GetValue<bool>("EnableSwaggerInProduction") || 
                        string.Equals(Environment.GetEnvironmentVariable("ENABLE_SWAGGER"), "true", StringComparison.OrdinalIgnoreCase);

    if (enableSwagger)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "RealEstateApp API v1");
            c.RoutePrefix = "swagger"; // Permite abrir swagger en /swagger
        });
    }
    else
    {
        app.UseHttpsRedirection();
        app.UseHsts();
    }

    app.UseStaticFiles();

    app.UseRouting();
    app.UseCors("AllowSpecificOrigins");
    app.UseRateLimiter();
    app.UseRequestLocalization();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<ChatHub>("/hubs/chat");
    app.MapHub<NotificationHub>("/hubs/notifications");

    Console.WriteLine("=================================================");
    Console.WriteLine("🚀 RealEstateApp WebApi iniciada exitosamente.");
    Console.WriteLine("🌐 Swagger: http://localhost:5196/swagger");
    Console.WriteLine("=================================================");

    app.Run();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"❌ ERROR FATAL AL INICIAR WEBAPI: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    Console.ResetColor();
    throw;
}
