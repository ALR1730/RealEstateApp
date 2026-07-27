using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RealEstateApp.Core.Application.ViewModels.Agent;

namespace RealEstateApp.Core.Application.ViewModels.Property
{
    /// <summary>
    /// ViewModel para la reasignación de una propiedad a un agente por parte del Administrador.
    /// </summary>
    public class ReassignPropertyViewModel
    {
        [Required]
        public int PropertyId { get; set; }

        public string PropertyCode { get; set; } = string.Empty;
        public string PropertyTypeName { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public string CurrentAgentId { get; set; } = string.Empty;
        public string CurrentAgentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un agente")]
        [Display(Name = "Nuevo Agente Asignado")]
        public string NewAgentId { get; set; } = string.Empty;

        public List<AgentViewModel> Agents { get; set; } = new();
    }
}
