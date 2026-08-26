using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface ILeadPipelineRepository : IGenericRepository<LeadPipeline>
    {
        Task<List<LeadPipeline>> GetByAgentIdAsync(string agentId);
        Task<List<LeadPipeline>> GetByAgentAndStageAsync(string agentId, string stage);
        Task<LeadPipeline?> GetByIdWithPropertyAsync(int id);
    }
}
