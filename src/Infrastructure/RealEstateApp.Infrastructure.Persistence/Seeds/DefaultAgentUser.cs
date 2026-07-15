using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Infrastructure.Persistence.Seeds
{
    /// <summary>
    /// Crea el usuario agente por defecto.
    /// Email: agent@realestate.com | Password: Agent123!
    /// </summary>
    public static class DefaultAgentUser
    {
        public static async Task SeedAsync(UserManager<IdentityUser> userManager)
        {
            var defaultUser = new IdentityUser
            {
                UserName = "agentuser",
                Email = "agent@realestate.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                var result = await userManager.CreateAsync(defaultUser, "Agent123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(defaultUser, Roles.Agent.ToString());
                }
            }
        }
    }
}
