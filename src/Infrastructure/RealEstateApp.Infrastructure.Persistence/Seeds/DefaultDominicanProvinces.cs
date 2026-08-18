using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Seeds
{
    /// <summary>
    /// Semilla oficial con las 31 provincias y el Distrito Nacional de la República Dominicana,
    /// junto con sus municipios cabecera y principales municipios.
    /// </summary>
    public static class DefaultDominicanProvinces
    {
        public static async Task SeedAsync(ApplicationDbContext dbContext)
        {
            if (!await dbContext.Provinces.AnyAsync())
            {
                var provinces = new List<Province>
                {
                    new Province
                    {
                        Name = "Distrito Nacional",
                        IsoCode = "DO-01",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Santo Domingo de Guzmán" }
                        }
                    },
                    new Province
                    {
                        Name = "Santo Domingo",
                        IsoCode = "DO-32",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Santo Domingo Este" },
                            new Municipality { Name = "Santo Domingo Norte" },
                            new Municipality { Name = "Santo Domingo Oeste" },
                            new Municipality { Name = "Boca Chica" },
                            new Municipality { Name = "Los Alcarrizos" },
                            new Municipality { Name = "Pedro Brand" },
                            new Municipality { Name = "San Antonio de Guerra" }
                        }
                    },
                    new Province
                    {
                        Name = "Santiago",
                        IsoCode = "DO-25",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Santiago de los Caballeros" },
                            new Municipality { Name = "Bisonó (Navarrete)" },
                            new Municipality { Name = "Jánico" },
                            new Municipality { Name = "Licey al Medio" },
                            new Municipality { Name = "Puñal" },
                            new Municipality { Name = "Sabana Iglesia" },
                            new Municipality { Name = "San José de las Matas" },
                            new Municipality { Name = "Tamboril" },
                            new Municipality { Name = "Villa González" }
                        }
                    },
                    new Province
                    {
                        Name = "La Altagracia",
                        IsoCode = "DO-11",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Higüey" },
                            new Municipality { Name = "San Rafael del Yuma" },
                            new Municipality { Name = "Punta Cana / Bávaro" },
                            new Municipality { Name = "Bayahíbe" }
                        }
                    },
                    new Province
                    {
                        Name = "Puerto Plata",
                        IsoCode = "DO-18",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "San Felipe de Puerto Plata" },
                            new Municipality { Name = "Altamira" },
                            new Municipality { Name = "Guananico" },
                            new Municipality { Name = "Imbert" },
                            new Municipality { Name = "Los Hidalgos" },
                            new Municipality { Name = "Luperón" },
                            new Municipality { Name = "Sosúa" },
                            new Municipality { Name = "Cabarete" },
                            new Municipality { Name = "Villa Isabela" },
                            new Municipality { Name = "Villa Montellano" }
                        }
                    },
                    new Province
                    {
                        Name = "La Romana",
                        IsoCode = "DO-12",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "La Romana" },
                            new Municipality { Name = "Guaymate" },
                            new Municipality { Name = "Villa Hermosa" }
                        }
                    },
                    new Province
                    {
                        Name = "La Vega",
                        IsoCode = "DO-13",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Concepción de La Vega" },
                            new Municipality { Name = "Constanza" },
                            new Municipality { Name = "Jarabacoa" },
                            new Municipality { Name = "Jima Abajo" }
                        }
                    },
                    new Province
                    {
                        Name = "Samaná",
                        IsoCode = "DO-20",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Santa Bárbara de Samaná" },
                            new Municipality { Name = "Las Terrenas" },
                            new Municipality { Name = "Sánchez" }
                        }
                    },
                    new Province
                    {
                        Name = "San Cristóbal",
                        IsoCode = "DO-21",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "San Cristóbal" },
                            new Municipality { Name = "Bajos de Haina" },
                            new Municipality { Name = "Cambita Garabitos" },
                            new Municipality { Name = "Los Cacaos" },
                            new Municipality { Name = "Sabana Grande de Palenque" },
                            new Municipality { Name = "San Gregorio de Nigua" },
                            new Municipality { Name = "Villa Altagracia" },
                            new Municipality { Name = "Yaguate" }
                        }
                    },
                    new Province
                    {
                        Name = "San Pedro de Macorís",
                        IsoCode = "DO-23",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "San Pedro de Macorís" },
                            new Municipality { Name = "Consuelo" },
                            new Municipality { Name = "Guayacanes" },
                            new Municipality { Name = "Quisqueya" },
                            new Municipality { Name = "Ramón Santana" },
                            new Municipality { Name = "San José de Los Llanos" }
                        }
                    },
                    new Province
                    {
                        Name = "Duarte",
                        IsoCode = "DO-06",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "San Francisco de Macorís" },
                            new Municipality { Name = "Arenoso" },
                            new Municipality { Name = "Castillo" },
                            new Municipality { Name = "Eugenio María de Hostos" },
                            new Municipality { Name = "Las Guáranas" },
                            new Municipality { Name = "Pimentel" },
                            new Municipality { Name = "Villa Riva" }
                        }
                    },
                    new Province
                    {
                        Name = "Espaillat",
                        IsoCode = "DO-09",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Moca" },
                            new Municipality { Name = "Cayetano Germosén" },
                            new Municipality { Name = "Gaspar Hernández" },
                            new Municipality { Name = "Jamao al Norte" }
                        }
                    },
                    new Province
                    {
                        Name = "Peravia",
                        IsoCode = "DO-17",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Baní" },
                            new Municipality { Name = "Matanzas" },
                            new Municipality { Name = "Nizao" }
                        }
                    },
                    new Province
                    {
                        Name = "Azua",
                        IsoCode = "DO-02",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Azua de Compostela" },
                            new Municipality { Name = "Estebanía" },
                            new Municipality { Name = "Guayabal" },
                            new Municipality { Name = "Las Charcas" },
                            new Municipality { Name = "Padre Las Casas" },
                            new Municipality { Name = "Peralta" },
                            new Municipality { Name = "Sabana Yegua" }
                        }
                    },
                    new Province
                    {
                        Name = "Barahona",
                        IsoCode = "DO-04",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Santa Cruz de Barahona" },
                            new Municipality { Name = "Cabral" },
                            new Municipality { Name = "Enriquillo" },
                            new Municipality { Name = "Paraíso" },
                            new Municipality { Name = "Vicente Noble" }
                        }
                    },
                    new Province
                    {
                        Name = "Monseñor Nouel",
                        IsoCode = "DO-28",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Bonao" },
                            new Municipality { Name = "Maimón" },
                            new Municipality { Name = "Piedra Blanca" }
                        }
                    },
                    new Province
                    {
                        Name = "Monte Plata",
                        IsoCode = "DO-29",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Monte Plata" },
                            new Municipality { Name = "Bayaguana" },
                            new Municipality { Name = "Esperalvillo" },
                            new Municipality { Name = "Sabana Grande de Boyá" },
                            new Municipality { Name = "Yamasá" }
                        }
                    },
                    new Province
                    {
                        Name = "María Trinidad Sánchez",
                        IsoCode = "DO-14",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Nagua" },
                            new Municipality { Name = "Cabrera" },
                            new Municipality { Name = "El Factor" },
                            new Municipality { Name = "Río San Juan" }
                        }
                    },
                    new Province
                    {
                        Name = "Sánchez Ramírez",
                        IsoCode = "DO-24",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Cotuí" },
                            new Municipality { Name = "Cevicos" },
                            new Municipality { Name = "Fantino" },
                            new Municipality { Name = "La Mata" }
                        }
                    },
                    new Province
                    {
                        Name = "San Juan",
                        IsoCode = "DO-22",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "San Juan de la Maguana" },
                            new Municipality { Name = "Bohechío" },
                            new Municipality { Name = "El Cercado" },
                            new Municipality { Name = "Juan de Herrera" },
                            new Municipality { Name = "Las Matas de Farfán" },
                            new Municipality { Name = "Vallejo" }
                        }
                    },
                    new Province
                    {
                        Name = "Hato Mayor",
                        IsoCode = "DO-30",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Hato Mayor del Rey" },
                            new Municipality { Name = "El Valle" },
                            new Municipality { Name = "Sabana de la Mar" }
                        }
                    },
                    new Province
                    {
                        Name = "El Seibo",
                        IsoCode = "DO-08",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Santa Cruz de El Seibo" },
                            new Municipality { Name = "Miches" }
                        }
                    },
                    new Province
                    {
                        Name = "Hermanas Mirabal",
                        IsoCode = "DO-19",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Salcedo" },
                            new Municipality { Name = "Tenares" },
                            new Municipality { Name = "Villa Tapia" }
                        }
                    },
                    new Province
                    {
                        Name = "Valverde",
                        IsoCode = "DO-27",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Mao" },
                            new Municipality { Name = "Esperanza" },
                            new Municipality { Name = "Laguna Salada" }
                        }
                    },
                    new Province
                    {
                        Name = "Monte Cristi",
                        IsoCode = "DO-15",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "San Fernando de Monte Cristi" },
                            new Municipality { Name = "Castañuelas" },
                            new Municipality { Name = "Guayubín" },
                            new Municipality { Name = "Las Matas de Santa Cruz" },
                            new Municipality { Name = "Pepillo Salcedo" },
                            new Municipality { Name = "Villa Vásquez" }
                        }
                    },
                    new Province
                    {
                        Name = "Dajabón",
                        IsoCode = "DO-05",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Dajabón" },
                            new Municipality { Name = "El Pino" },
                            new Municipality { Name = "Loma de Cabrera" },
                            new Municipality { Name = "Partido" },
                            new Municipality { Name = "Restauración" }
                        }
                    },
                    new Province
                    {
                        Name = "Santiago Rodríguez",
                        IsoCode = "DO-26",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "San Ignacio de Sabaneta" },
                            new Municipality { Name = "Monción" },
                            new Municipality { Name = "Villa Los Almácigos" }
                        }
                    },
                    new Province
                    {
                        Name = "San José de Ocoa",
                        IsoCode = "DO-31",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "San José de Ocoa" },
                            new Municipality { Name = "Rancho Arriba" },
                            new Municipality { Name = "Sabana Larga" }
                        }
                    },
                    new Province
                    {
                        Name = "Pedernales",
                        IsoCode = "DO-16",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Pedernales" },
                            new Municipality { Name = "Oviedo" },
                            new Municipality { Name = "Bahía de las Águilas" }
                        }
                    },
                    new Province
                    {
                        Name = "Independencia",
                        IsoCode = "DO-10",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Jimaní" },
                            new Municipality { Name = "Duvergé" },
                            new Municipality { Name = "La Descubierta" },
                            new Municipality { Name = "Mella" }
                        }
                    },
                    new Province
                    {
                        Name = "Bahoruco",
                        IsoCode = "DO-03",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Neiba" },
                            new Municipality { Name = "Galván" },
                            new Municipality { Name = "Los Ríos" },
                            new Municipality { Name = "Tamayo" },
                            new Municipality { Name = "Villa Jaragua" }
                        }
                    },
                    new Province
                    {
                        Name = "Elías Piña",
                        IsoCode = "DO-07",
                        Municipalities = new List<Municipality>
                        {
                            new Municipality { Name = "Comendador" },
                            new Municipality { Name = "Bánica" },
                            new Municipality { Name = "El Llano" },
                            new Municipality { Name = "Hondo Valle" }
                        }
                    }
                };

                await dbContext.Provinces.AddRangeAsync(provinces);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
