using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Infrastructure.Persistence.Seeds
{
    /// <summary>
    /// Crea los usuarios clientes por defecto.
    /// Client 1: client@realestate.com | Password: Client123!
    /// Client 2: client2@realestate.com | Password: Client123!
    /// </summary>
    public static class DefaultClientUser
    {
        public static async Task SeedAsync(UserManager<IdentityUser> userManager)
        {
            var clients = new List<(string userName, string email, string phone)>
            {
                ("clientuser", "client@realestate.com", "809-555-0301"),
                ("clientuser2", "client2@realestate.com", "809-555-0302")
            };

            foreach (var (userName, email, phone) in clients)
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
                    var result = await userManager.CreateAsync(newUser, "Client123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newUser, Roles.Client.ToString());
                    }
                }
            }
        }
    }
}
