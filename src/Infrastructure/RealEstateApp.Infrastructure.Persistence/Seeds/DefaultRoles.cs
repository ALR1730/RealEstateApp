using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Infrastructure.Persistence.Seeds
{
    /// <summary>
    /// Inicializador de los roles del sistema.
    /// Crea los 4 roles requeridos: Administrador, Agente, Cliente y Desarrollador.
    /// </summary>
    public static class DefaultRoles
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            // Crear roles si no existen
            if (!await roleManager.RoleExistsAsync(Roles.Admin.ToString()))
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));

            if (!await roleManager.RoleExistsAsync(Roles.Agent.ToString()))
                await roleManager.CreateAsync(new IdentityRole(Roles.Agent.ToString()));

            if (!await roleManager.RoleExistsAsync(Roles.Client.ToString()))
                await roleManager.CreateAsync(new IdentityRole(Roles.Client.ToString()));

            if (!await roleManager.RoleExistsAsync(Roles.Developer.ToString()))
                await roleManager.CreateAsync(new IdentityRole(Roles.Developer.ToString()));
        }
    }
}
