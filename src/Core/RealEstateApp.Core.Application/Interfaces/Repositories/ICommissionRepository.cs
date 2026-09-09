using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface ICommissionRepository : IGenericRepository<Commission>
    {
        /// <summary>
        /// Obtiene todas las comisiones de un agente (con propiedad y oferta).
        /// </summary>
        Task<List<Commission>> GetByAgentIdAsync(string agentId);

        /// <summary>
        /// Obtiene todas las comisiones del sistema (con propiedad y oferta).
        /// </summary>
        Task<List<Commission>> GetAllWithDetailsAsync();
    }
}
