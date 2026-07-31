using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Seeds
{
    /// <summary>
    /// Pobla la base de datos con información completa de prueba:
    /// Tipos de propiedades, Tipos de venta, Mejoras, Propiedades con imágenes,
    /// Favoritos de clientes, Ofertas activas/procesadas e Hilos de Chat.
    /// </summary>
    public static class DefaultRealEstateData
    {
        public static async Task SeedAsync(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            // 1. Tipos de Propiedades
            if (!await dbContext.PropertyTypes.AnyAsync())
            {
                var propertyTypes = new List<PropertyType>
                {
                    new PropertyType { Name = "Apartamento", Description = "Unidades residenciales en edificios multifamiliares o torres." },
                    new PropertyType { Name = "Villa", Description = "Residencias de lujo independientes con amplias áreas verdes." },
                    new PropertyType { Name = "Casa", Description = "Viviendas unifamiliares con terrenos y jardines privados." },
                    new PropertyType { Name = "Penthouse", Description = "Apartamentos exclusivos en el último piso con terraza y vista panorámica." },
                    new PropertyType { Name = "Terreno", Description = "Lotes de tierra aptos para construcción comercial o residencial." }
                };
                await dbContext.PropertyTypes.AddRangeAsync(propertyTypes);
                await dbContext.SaveChangesAsync();
            }

            // 2. Tipos de Ventas
            if (!await dbContext.SaleTypes.AnyAsync())
            {
                var saleTypes = new List<SaleType>
                {
                    new SaleType { Name = "Venta Directa", Description = "Adquisición de la propiedad en un pago único o financiamiento." },
                    new SaleType { Name = "Alquiler", Description = "Renta mensual a largo plazo para vivienda o negocio." },
                    new SaleType { Name = "Alquiler Vacacional", Description = "Renta por días o semanas para estadías cortas." }
                };
                await dbContext.SaleTypes.AddRangeAsync(saleTypes);
                await dbContext.SaveChangesAsync();
            }

            // 3. Mejoras
            if (!await dbContext.Improvements.AnyAsync())
            {
                var improvements = new List<Improvement>
                {
                    new Improvement { Name = "Piscina", Description = "Piscina privada o común." },
                    new Improvement { Name = "Planta Eléctrica", Description = "Generador eléctrico full de emergencia." },
                    new Improvement { Name = "Jacuzzi", Description = "Tena de hidromasajes." },
                    new Improvement { Name = "Gimnasio", Description = "Área equipada para ejercicios." },
                    new Improvement { Name = "Seguridad 24/7", Description = "Vigilancia y control de acceso constante." },
                    new Improvement { Name = "Ascensor", Description = "Elevadores de última generación." },
                    new Improvement { Name = "Balcón con vista al mar", Description = "Vista panorámica hacia el océano." },
                    new Improvement { Name = "Parqueo techado", Description = "Estacionamiento cubierto subterráneo o bajo techo." }
                };
                await dbContext.Improvements.AddRangeAsync(improvements);
                await dbContext.SaveChangesAsync();
            }

            // 4. Propiedades
            if (!await dbContext.Properties.AnyAsync())
            {
                var agent1 = await userManager.FindByEmailAsync("agent@realestate.com");
                var agent2 = await userManager.FindByEmailAsync("agent2@realestate.com");

                var aptType = await dbContext.PropertyTypes.FirstAsync(pt => pt.Name == "Apartamento");
                var villaType = await dbContext.PropertyTypes.FirstAsync(pt => pt.Name == "Villa");
                var casaType = await dbContext.PropertyTypes.FirstAsync(pt => pt.Name == "Casa");
                var penthouseType = await dbContext.PropertyTypes.FirstAsync(pt => pt.Name == "Penthouse");

                var ventaType = await dbContext.SaleTypes.FirstAsync(st => st.Name == "Venta Directa");
                var alquilerType = await dbContext.SaleTypes.FirstAsync(st => st.Name == "Alquiler");

                if (agent1 != null && agent2 != null)
                {
                    var prop1 = new Property
                    {
                        Name = "Apartamento Moderno Bella Vista",
                        Code = "APT101",
                        Price = 195000.00m,
                        Rooms = 3,
                        Bathrooms = 2,
                        SizeInMeters = 135.50m,
                        Description = "Hermoso apartamento moderno en Bella Vista con excelente iluminación natural, pisos de porcelanato y balcón amplio.",
                        PropertyTypeId = aptType.Id,
                        SaleTypeId = ventaType.Id,
                        AgentId = agent1.Id,
                        Latitude = 18.4485,
                        Longitude = -69.9405,
                        VideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                        Tour360Url = "https://matterport.com/discover",
                        MontoSeparacion = 5000.00m,
                        PorcentajeInicialRequerido = 20,
                        Status = PropertyStatus.Available
                    };

                    var prop2 = new Property
                    {
                        Name = "Villa Frente al Mar Cap Cana",
                        Code = "VIL202",
                        Price = 450000.00m,
                        Rooms = 5,
                        Bathrooms = 4,
                        SizeInMeters = 380.00m,
                        Description = "Espectacular Villa frente al mar en Cap Cana con piscina infinity, acabados de mármol y muelle privado.",
                        PropertyTypeId = villaType.Id,
                        SaleTypeId = ventaType.Id,
                        AgentId = agent1.Id,
                        Latitude = 18.4975,
                        Longitude = -68.3720,
                        VideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                        Tour360Url = "https://matterport.com/discover",
                        MontoSeparacion = 15000.00m,
                        PorcentajeInicialRequerido = 30,
                        Status = PropertyStatus.Available
                    };

                    var prop3 = new Property
                    {
                        Name = "Residencia Familiar Arroyo Hondo",
                        Code = "CAS303",
                        Price = 280000.00m,
                        Rooms = 4,
                        Bathrooms = 3,
                        SizeInMeters = 250.00m,
                        Description = "Acogedora residencia familiar en Arroyo Hondo con amplio patio trasero, terraza techada y zona residencial tranquila.",
                        PropertyTypeId = casaType.Id,
                        SaleTypeId = alquilerType.Id,
                        AgentId = agent2.Id,
                        Latitude = 18.4950,
                        Longitude = -69.9200,
                        VideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                        Tour360Url = "https://matterport.com/discover",
                        MontoSeparacion = 2500.00m,
                        PorcentajeInicialRequerido = 10,
                        Status = PropertyStatus.Available
                    };

                    var prop4 = new Property
                    {
                        Name = "Penthouse Exclusivo Piantini",
                        Code = "PNT404",
                        Price = 520000.00m,
                        Rooms = 4,
                        Bathrooms = 4,
                        SizeInMeters = 310.00m,
                        Description = "Penthouse de dos niveles en Piantini con Jacuzzi privado en la terraza y vista de 360 grados a la ciudad.",
                        PropertyTypeId = penthouseType.Id,
                        SaleTypeId = ventaType.Id,
                        AgentId = agent2.Id,
                        Latitude = 18.4720,
                        Longitude = -69.9320,
                        VideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                        Tour360Url = "https://matterport.com/discover",
                        MontoSeparacion = 20000.00m,
                        PorcentajeInicialRequerido = 25,
                        Status = PropertyStatus.Sold
                    };

                    await dbContext.Properties.AddRangeAsync(prop1, prop2, prop3, prop4);
                    await dbContext.SaveChangesAsync();

                    // 4.1 Imágenes de las propiedades
                    var images = new List<PropertyImage>
                    {
                        new PropertyImage { PropertyId = prop1.Id, ImageUrl = "https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?auto=format&fit=crop&w=800&q=80" },
                        new PropertyImage { PropertyId = prop1.Id, ImageUrl = "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?auto=format&fit=crop&w=800&q=80" },

                        new PropertyImage { PropertyId = prop2.Id, ImageUrl = "https://images.unsplash.com/photo-1613977257363-707ba9348227?auto=format&fit=crop&w=800&q=80" },
                        new PropertyImage { PropertyId = prop2.Id, ImageUrl = "https://images.unsplash.com/photo-1613490493576-7fde63acd811?auto=format&fit=crop&w=800&q=80" },

                        new PropertyImage { PropertyId = prop3.Id, ImageUrl = "https://images.unsplash.com/photo-1580587771525-78b9dba3b914?auto=format&fit=crop&w=800&q=80" },
                        new PropertyImage { PropertyId = prop4.Id, ImageUrl = "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?auto=format&fit=crop&w=800&q=80" }
                    };
                    await dbContext.PropertyImages.AddRangeAsync(images);

                    // 4.2 Mejoras vinculadas a las propiedades
                    var pool = await dbContext.Improvements.FirstAsync(i => i.Name == "Piscina");
                    var plant = await dbContext.Improvements.FirstAsync(i => i.Name == "Planta Eléctrica");
                    var security = await dbContext.Improvements.FirstAsync(i => i.Name == "Seguridad 24/7");
                    var elevator = await dbContext.Improvements.FirstAsync(i => i.Name == "Ascensor");

                    var propImprovements = new List<PropertyImprovement>
                    {
                        new PropertyImprovement { PropertyId = prop1.Id, ImprovementId = plant.Id },
                        new PropertyImprovement { PropertyId = prop1.Id, ImprovementId = elevator.Id },
                        new PropertyImprovement { PropertyId = prop1.Id, ImprovementId = security.Id },

                        new PropertyImprovement { PropertyId = prop2.Id, ImprovementId = pool.Id },
                        new PropertyImprovement { PropertyId = prop2.Id, ImprovementId = plant.Id },
                        new PropertyImprovement { PropertyId = prop2.Id, ImprovementId = security.Id },

                        new PropertyImprovement { PropertyId = prop4.Id, ImprovementId = pool.Id },
                        new PropertyImprovement { PropertyId = prop4.Id, ImprovementId = elevator.Id }
                    };
                    await dbContext.PropertyImprovements.AddRangeAsync(propImprovements);
                    await dbContext.SaveChangesAsync();

                    // 5. Favoritos, Ofertas y Chats para Clientes
                    var client1 = await userManager.FindByEmailAsync("client@realestate.com");
                    var client2 = await userManager.FindByEmailAsync("client2@realestate.com");

                    if (client1 != null && client2 != null)
                    {
                        // 5.1 Favoritos
                        if (!await dbContext.Favorites.AnyAsync())
                        {
                            var favorites = new List<Favorite>
                            {
                                new Favorite { ClienteId = client1.Id, PropertyId = prop1.Id },
                                new Favorite { ClienteId = client1.Id, PropertyId = prop2.Id }
                            };
                            await dbContext.Favorites.AddRangeAsync(favorites);
                        }

                        // 5.2 Ofertas
                        if (!await dbContext.Offers.AnyAsync())
                        {
                            var offers = new List<Offer>
                            {
                                new Offer
                                {
                                    PropertyId = prop1.Id,
                                    ClienteId = client1.Id,
                                    MontoOfertado = 185000.00m,
                                    FechaOferta = DateTime.UtcNow.AddDays(-2),
                                    Status = RealEstateApp.Core.Domain.Enums.OfferStatus.Pending
                                },
                                new Offer
                                {
                                    PropertyId = prop4.Id,
                                    ClienteId = client2.Id,
                                    MontoOfertado = 510000.00m,
                                    FechaOferta = DateTime.UtcNow.AddDays(-5),
                                    Status = RealEstateApp.Core.Domain.Enums.OfferStatus.Accepted
                                }
                            };
                            await dbContext.Offers.AddRangeAsync(offers);
                        }

                        // 5.3 Chats de Mensajería
                        if (!await dbContext.Chats.AnyAsync())
                        {
                            var chats = new List<Chat>
                            {
                                new Chat
                                {
                                    PropertyId = prop1.Id,
                                    ClienteId = client1.Id,
                                    AgenteId = agent1.Id,
                                    SenderId = client1.Id,
                                    MessageContent = "¡Hola Carlos! Estoy muy interesado en el apartamento APT101. ¿Está disponible para visitas esta semana?",
                                    SentAt = DateTime.UtcNow.AddHours(-10)
                                },
                                new Chat
                                {
                                    PropertyId = prop1.Id,
                                    ClienteId = client1.Id,
                                    AgenteId = agent1.Id,
                                    SenderId = agent1.Id,
                                    MessageContent = "¡Hola Juan! Sí, con gusto podemos agendar una visita mañana a las 4:00 PM o el sábado en la mañana.",
                                    SentAt = DateTime.UtcNow.AddHours(-8)
                                },
                                new Chat
                                {
                                    PropertyId = prop1.Id,
                                    ClienteId = client1.Id,
                                    AgenteId = agent1.Id,
                                    SenderId = client1.Id,
                                    MessageContent = "Excelente, coordinemos para mañana a las 4:00 PM. ¡Muchas gracias!",
                                    SentAt = DateTime.UtcNow.AddHours(-6)
                                }
                            };
                            await dbContext.Chats.AddRangeAsync(chats);
                        }

                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
