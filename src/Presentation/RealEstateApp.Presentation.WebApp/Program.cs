using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Application;
using RealEstateApp.Infrastructure.Persistence;
using RealEstateApp.Infrastructure.Persistence.Seeds;
using RealEstateApp.Infrastructure.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registrar capas de la arquitectura Onion
builder.Services.AddApplicationLayer();
builder.Services.AddPersistenceInfrastructure(builder.Configuration);
builder.Services.AddSharedInfrastructure();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// Ejecutar Seeds al iniciar la aplicación
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        await DefaultRoles.SeedAsync(roleManager);
        await DefaultAdminUser.SeedAsync(userManager);
        await DefaultAgentUser.SeedAsync(userManager);
        await DefaultClientUser.SeedAsync(userManager);
        await DefaultDeveloperUser.SeedAsync(userManager);
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
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
