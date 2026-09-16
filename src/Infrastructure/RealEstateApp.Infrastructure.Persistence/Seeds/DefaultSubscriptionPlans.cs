using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Seeds
{
    public static class DefaultSubscriptionPlans
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.SubscriptionPlans.AnyAsync())
            {
                var plans = new List<SubscriptionPlan>
                {
                    new SubscriptionPlan
                    {
                        Name = "Gratuito / Starter",
                        Description = "Ideal para agentes que están comenzando o publican inmuebles de forma esporádica.",
                        MonthlyPrice = 0.00m,
                        MaxActiveProperties = 3,
                        MaxFeaturedProperties = 0,
                        Allows3DTours = true,
                        AllowsVideo = true,
                        CommissionPercentage = 5.00m,
                        IsActive = true
                    },
                    new SubscriptionPlan
                    {
                        Name = "Profesional (Pro)",
                        Description = "Diseñado para agentes activos que requieren mayor visibilidad y listados destacados.",
                        MonthlyPrice = 49.99m,
                        MaxActiveProperties = 15,
                        MaxFeaturedProperties = 3,
                        Allows3DTours = true,
                        AllowsVideo = true,
                        CommissionPercentage = 4.50m,
                        IsActive = true
                    },
                    new SubscriptionPlan
                    {
                        Name = "Inmobiliaria (Premium)",
                        Description = "Para firmas inmobiliarias y top producers con alto volumen de inventario y máxima exposición.",
                        MonthlyPrice = 129.99m,
                        MaxActiveProperties = 50,
                        MaxFeaturedProperties = 10,
                        Allows3DTours = true,
                        AllowsVideo = true,
                        CommissionPercentage = 3.50m,
                        IsActive = true
                    }
                };

                await context.SubscriptionPlans.AddRangeAsync(plans);
                await context.SaveChangesAsync();
            }
        }
    }
}
