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
            var agents = new List<(string userName, string email, string phone, string firstName, string lastName, bool isActive)>
            {
                ("agentuser", "agent@realestate.com", "809-555-0201", "Carlos", "Mendoza", true),
                ("agentuser2", "agent2@realestate.com", "809-555-0202", "Ana", "Gómez", true),
                ("agentuser3", "agent3@realestate.com", "809-555-0203", "Pedro", "Martínez", false)
            };

            foreach (var (userName, email, phone, firstName, lastName, isActive) in agents)
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
                        await userManager.AddClaimAsync(newUser, new System.Security.Claims.Claim("FirstName", firstName));
                        await userManager.AddClaimAsync(newUser, new System.Security.Claims.Claim("LastName", lastName));
                    }
                }
                else
                {
                    var claims = await userManager.GetClaimsAsync(user);
                    if (!claims.Any(c => c.Type == "FirstName"))
                    {
                        await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FirstName", firstName));
                    }
                    if (!claims.Any(c => c.Type == "LastName"))
                    {
                        await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("LastName", lastName));
                    }
                }
            }
        }
    }
}
