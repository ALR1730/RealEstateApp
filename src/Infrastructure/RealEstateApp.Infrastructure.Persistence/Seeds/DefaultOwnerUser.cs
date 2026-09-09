using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Infrastructure.Persistence.Seeds
{
    /// <summary>
    /// Crea los usuarios propietarios por defecto.
    /// Owner 1: owner@realestate.com | Password: Owner123!
    /// </summary>
    public static class DefaultOwnerUser
    {
        public static async Task SeedAsync(UserManager<IdentityUser> userManager)
        {
            var owners = new List<(string userName, string email, string phone)>
            {
                ("owneruser", "owner@realestate.com", "809-555-0501")
            };

            foreach (var (userName, email, phone) in owners)
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
                    var result = await userManager.CreateAsync(newUser, "Owner123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newUser, Roles.Owner.ToString());
                    }
                }
            }
        }
    }
}
