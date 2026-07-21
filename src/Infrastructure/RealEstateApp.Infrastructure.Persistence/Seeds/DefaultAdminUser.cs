using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Infrastructure.Persistence.Seeds
{
    /// <summary>
    /// Crea los usuarios administradores por defecto.
    /// Email: admin@realestate.com | Password: Admin123!
    /// Email: admin2@realestate.com | Password: Admin123!
    /// </summary>
    public static class DefaultAdminUser
    {
        public static async Task SeedAsync(UserManager<IdentityUser> userManager)
        {
            var admins = new List<(string userName, string email, string phone)>
            {
                ("adminuser", "admin@realestate.com", "809-555-0101"),
                ("adminuser2", "admin2@realestate.com", "809-555-0102")
            };

            foreach (var (userName, email, phone) in admins)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    var newUser = new IdentityUser
                    {
                        UserName = userName,
                        Email = email,
                        PhoneNumber = phone,
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true
                    };
                    var result = await userManager.CreateAsync(newUser, "Admin123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newUser, Roles.Admin.ToString());
                    }
                }
            }
        }
    }
}
