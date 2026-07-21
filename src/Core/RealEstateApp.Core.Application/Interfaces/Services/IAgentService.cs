using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.DTOs.Agent;
using RealEstateApp.Core.Application.ViewModels.Agent;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio de aplicación para gestión y consulta de agentes inmobiliarios.
    /// </summary>
    public interface IAgentService
    {
        Task<List<AgentViewModel>> GetAllViewModelAsync();
        Task<AgentViewModel?> GetByIdViewModelAsync(string id);
        Task<List<AgentDto>> GetAllDtoAsync();
        Task<AgentDto?> GetByIdDtoAsync(string id);
        Task ChangeStatusAsync(string agentId, bool isActive);
    }
}
