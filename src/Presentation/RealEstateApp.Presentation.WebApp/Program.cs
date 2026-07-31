using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application;
using RealEstateApp.Infrastructure.Persistence;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Seeds;
using RealEstateApp.Infrastructure.Shared;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configurar cultura por defecto para República Dominicana (es-DO con símbolo RD$)
var defaultCulture = new System.Globalization.CultureInfo("es-DO");
defaultCulture.NumberFormat.CurrencySymbol = "RD$ ";
defaultCulture.NumberFormat.CurrencyPositivePattern = 0;
defaultCulture.NumberFormat.CurrencyNegativePattern = 0;
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

// Configurar HSTS con Preload y subdominios para producción
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// Configurar Rate Limiting para prevenir ataques de fuerza bruta y DDoS
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("WebhookPolicy", opt =>
    {
        opt.PermitLimit = 60;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

// Registrar capas de la arquitectura Onion
builder.Services.AddApplicationLayer();
builder.Services.AddPersistenceInfrastructure(builder.Configuration);
builder.Services.AddSharedInfrastructure();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Registrar autenticación con Google solo si las credenciales están configuradas
// Las credenciales deben estar en User Secrets (desarrollo) o variables de entorno (producción)
// Nunca hardcodear credenciales en appsettings.json
var googleClientId = builder.Configuration["GoogleAuth:ClientId"];
var googleClientSecret = builder.Configuration["GoogleAuth:ClientSecret"];

if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
            options.Events.OnRemoteFailure = context =>
            {
                var errorMessage = context.Failure?.Message ?? "Error al autenticar con el proveedor externo.";
                context.Response.Redirect($"/Account/Login?remoteError={Uri.EscapeDataString(errorMessage)}");
                context.HandleResponse();
                return Task.CompletedTask;
            };
        });
}
else
{
    Console.WriteLine("⚠️  [Seguridad] Google OAuth no configurado. Para habilitarlo, ejecute:");
    Console.WriteLine("    dotnet user-secrets set \"GoogleAuth:ClientId\" \"<su-client-id>\"");
    Console.WriteLine("    dotnet user-secrets set \"GoogleAuth:ClientSecret\" \"<su-client-secret>\"");
}

var app = builder.Build();

// Ejecutar Seeds y Migraciones al iniciar la aplicación
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        await DefaultRoles.SeedAsync(roleManager);

        // Seeding de usuarios de prueba y datos de demostración solo en ambiente de desarrollo
        if (app.Environment.IsDevelopment())
        {
            await DefaultAdminUser.SeedAsync(userManager);
            await DefaultAgentUser.SeedAsync(userManager);
            await DefaultClientUser.SeedAsync(userManager);
            await DefaultDeveloperUser.SeedAsync(userManager);
            await DefaultRealEstateData.SeedAsync(dbContext, userManager);
        }
    }
    catch (Exception ex)
    {
        // Log del error de seed (en producción usar ILogger)
        Console.WriteLine($"Error al ejecutar los seeds: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

var supportedCultures = new[] { defaultCulture };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(defaultCulture),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
