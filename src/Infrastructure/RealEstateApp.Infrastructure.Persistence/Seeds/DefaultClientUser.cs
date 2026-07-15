using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Infrastructure.Persistence.Seeds
{
    /// <summary>
    /// Crea el usuario cliente por defecto.
    /// Email: client@realestate.com | Password: Client123!
    /// </summary>
    public static class DefaultClientUser
    {
        public static async Task SeedAsync(UserManager<IdentityUser> userManager)
        {
            var defaultUser = new IdentityUser
            {
                UserName = "clientuser",
                Email = "client@realestate.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                var result = await userManager.CreateAsync(defaultUser, "Client123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(defaultUser, Roles.Client.ToString());
                }
            }
        }
    }
}
