using System.Collections.Generic;

namespace RealEstateApp.Core.Application.DTOs.Dashboard
{
    /// <summary>
    /// Representa la cantidad de propiedades agrupadas por tipo.
    /// </summary>
    public class PropertyTypeCountDto
    {
        public string TypeName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    /// <summary>
    /// Métricas agregadas de propiedades calculadas directamente en base de datos.
    /// </summary>
    public class PropertyDashboardMetricsDto
    {
        public int TotalAvailableProperties { get; set; }
        public int TotalReservedProperties { get; set; }
        public int TotalSoldProperties { get; set; }
        public int TotalProperties { get; set; }
        public List<PropertyTypeCountDto> PropertiesByType { get; set; } = new();
    }

    /// <summary>
    /// Métricas agregadas de usuarios agrupadas por rol y estado.
    /// </summary>
    public class UserDashboardMetricsDto
    {
        public int TotalActiveAgents { get; set; }
        public int TotalInactiveAgents { get; set; }
        public int TotalClients { get; set; }
        public int TotalDevelopers { get; set; }
    }

    /// <summary>
    /// DTO consolidado de KPIs ejecutivos para el dashboard de administración.
    /// Mantiene compatibilidad exacta con el contrato esperado por el frontend.
    /// </summary>
    public class DashboardMetricsDto
    {
        public int TotalAvailableProperties { get; set; }
        public int TotalReservedProperties { get; set; }
        public int TotalSoldProperties { get; set; }
        public int TotalProperties { get; set; }
        public int TotalActiveAgents { get; set; }
        public int TotalInactiveAgents { get; set; }
        public int TotalClients { get; set; }
        public int TotalDevelopers { get; set; }
        public List<PropertyTypeCountDto> PropertiesByType { get; set; } = new();
    }
}
