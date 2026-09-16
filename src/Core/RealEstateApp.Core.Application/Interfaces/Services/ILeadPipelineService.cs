using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.LeadPipeline;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio de CRM Kanban para gestión de leads inmobiliarios.
    /// Permite mover leads entre etapas del pipeline de ventas.
    /// </summary>
    public interface ILeadPipelineService
    {
        Task<List<LeadPipelineDto>> GetAllByAgentAsync(string agentId);
        Task<List<LeadPipelineDto>> GetByStageAsync(string agentId, string stage);
        Task<LeadPipelineDto?> GetByIdAsync(int id);
        Task<LeadPipelineDto> CreateAsync(CreateLeadRequest request, string agentId);
        Task<LeadPipelineDto> UpdateAsync(int id, UpdateLeadRequest request);
        Task<LeadPipelineDto> MoveToStageAsync(int id, string newStage);
        Task<LeadPipelineDto> UpdateSortOrderAsync(int id, int newSortOrder);
        Task DeleteAsync(int id);
        Task<LeadPipelineStatsDto> GetStatsAsync(string agentId);
    }
}
