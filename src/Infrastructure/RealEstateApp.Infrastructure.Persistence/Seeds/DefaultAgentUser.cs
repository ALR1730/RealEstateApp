using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Infrastructure.Persistence.Seeds
{
    /// <summary>
    /// Crea los usuarios agentes por defecto (activos e inactivo/pendiente de aprobación).
    /// Agent 1: agent@realestate.com | Password: Agent123! (Activo)
    /// Agent 2: agent2@realestate.com | Password: Agent123! (Activo)
    /// Agent 3: agent3@realestate.com | Password: Agent123! (Inactivo / Pendiente aprobación Admin)
    /// </summary>
    public static class DefaultAgentUser
    {
        public static async Task SeedAsync(UserManager<IdentityUser> userManager)
        {
            var agents = new List<(string userName, string email, string phone, bool isActive)>
            {
                ("agentuser", "agent@realestate.com", "809-555-0201", true),
                ("agentuser2", "agent2@realestate.com", "809-555-0202", true),
                ("agentuser3", "agent3@realestate.com", "809-555-0203", false)
            };

            foreach (var (userName, email, phone, isActive) in agents)
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
                        PhoneNumberConfirmed = true,
                        LockoutEnabled = true,
                        LockoutEnd = isActive ? null : DateTimeOffset.MaxValue
                    };
                    var result = await userManager.CreateAsync(newUser, "Agent123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newUser, Roles.Agent.ToString());
                    }
                }
            }
        }
    }
}
