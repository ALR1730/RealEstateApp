using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.BuyAbility;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio de BuyAbility - Evaluación de Capacidad de Compra.
    /// Analiza el perfil financiero del cliente para determinar su poder adquisitivo.
    /// </summary>
    public interface IBuyAbilityService
    {
        /// <summary>
        /// Evalúa la capacidad de compra de un cliente basándose en su perfil financiero.
        /// </summary>
        Task<BuyAbilityResultDto> EvaluateAsync(string clientId, BuyAbilityRequestDto request);

        /// <summary>
        /// Obtiene la última evaluación de capacidad de compra de un cliente.
        /// </summary>
        Task<BuyAbilityResultDto?> GetLastEvaluationAsync(string clientId);
    }
}
