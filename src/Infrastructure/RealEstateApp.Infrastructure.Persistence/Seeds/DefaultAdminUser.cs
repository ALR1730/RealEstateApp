using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Infrastructure.Persistence.Seeds
{
    /// <summary>
    /// Crea el usuario administrador por defecto.
    /// Email: admin@realestate.com | Password: Admin123!
    /// </summary>
    public static class DefaultAdminUser
    {
        public static async Task SeedAsync(UserManager<IdentityUser> userManager)
        {
            var defaultUser = new IdentityUser
            {
                UserName = "adminuser",
                Email = "admin@realestate.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                var result = await userManager.CreateAsync(defaultUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
                }
            }
        }
    }
}
