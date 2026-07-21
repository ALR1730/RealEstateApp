using System.Collections.Generic;

namespace RealEstateApp.Core.Application.ViewModels.Agent
{
    /// <summary>
    /// ViewModel para el directorio de agentes en la WebApp MVC.
    /// </summary>
    public class AgentViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Cantidad de propiedades gestionadas por el agente.
        /// </summary>
        public int PropertiesCount { get; set; }

        /// <summary>
        /// Propiedades del agente (para la vista de portafolio).
        /// </summary>
        public List<Property.PropertyViewModel>? Properties { get; set; }
    }
}
