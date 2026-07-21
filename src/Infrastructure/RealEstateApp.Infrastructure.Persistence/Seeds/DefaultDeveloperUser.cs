using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Infrastructure.Persistence.Seeds
{
    /// <summary>
    /// Crea los usuarios desarrolladores por defecto para la Web API REST.
    /// Developer 1: developer@realestate.com | Password: Developer123!
    /// Developer 2: dev2@realestate.com | Password: Developer123!
    /// </summary>
    public static class DefaultDeveloperUser
    {
        public static async Task SeedAsync(UserManager<IdentityUser> userManager)
        {
            var developers = new List<(string userName, string email, string phone)>
            {
                ("developeruser", "developer@realestate.com", "809-555-0401"),
                ("developeruser2", "dev2@realestate.com", "809-555-0402")
            };

            foreach (var (userName, email, phone) in developers)
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
                    var result = await userManager.CreateAsync(newUser, "Developer123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newUser, Roles.Developer.ToString());
                    }
                }
            }
        }
    }
}
