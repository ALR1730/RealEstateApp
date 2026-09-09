using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Commission;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    public interface ICommissionService
    {
        /// <summary>
        /// Crea el registro de comisión del agente cuando se acepta una oferta.
        /// Calcula la tasa según el plan de suscripción del agente (o default global).
        /// </summary>
        Task CreateForAcceptedOfferAsync(int offerId);

        /// <summary>
        /// Comisiones del agente autenticado.
        /// </summary>
        Task<List<CommissionViewModel>> GetByAgentAsync(string agentId, bool fetchDetails = true);

        /// <summary>
        /// Resumen de comisiones del agente (total, pendiente, pagada).
        /// </summary>
        Task<CommissionSummaryViewModel> GetAgentSummaryAsync(string agentId);

        /// <summary>
        /// Todas las comisiones del sistema (para el administrador).
        /// </summary>
        Task<List<CommissionViewModel>> GetAllAsync();

        /// <summary>
        /// Marca una comisión como pagada (acción del administrador).
        /// </summary>
        Task MarkAsPaidAsync(int commissionId);
    }
}