using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Offer;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    [Authorize]
    public class OffersController : BaseApiController
    {
        private readonly IOfferService _offerService;
        private readonly IPropertyService _propertyService;

        public OffersController(IOfferService offerService, IPropertyService propertyService)
        {
            _offerService = offerService;
            _propertyService = propertyService;
        }

        /// <summary>
        /// Obtiene el listado de ofertas realizadas por el cliente autenticado.
        /// </summary>
        [Authorize(Roles = "Client")]
        [HttpGet("my-offers")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<OfferViewModel>))]
        public async Task<IActionResult> GetMyOffersAsync()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            var offers = await _offerService.GetByClienteId(clientId);
            return Ok(offers);
        }

        /// <summary>
        /// Obtiene el listado de ofertas recibidas en las propiedades del agente autenticado.
        /// </summary>
        [Authorize(Roles = "Agent")]
        [HttpGet("received")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<OfferViewModel>))]
        public async Task<IActionResult> GetReceivedOffersAsync()
        {
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(agentId)) return Unauthorized();

            var agentProperties = await _propertyService.GetByAgentId(agentId);
            var propertyIds = agentProperties.ConvertAll(p => p.Id);
            var offers = await _offerService.GetByPropertyIds(propertyIds);
            return Ok(offers);
        }

        /// <summary>
        /// Envía una nueva oferta económica para una propiedad (Cliente).
        /// </summary>
        [Authorize(Roles = "Client")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SaveOfferViewModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MakeOfferAsync([FromBody] SaveOfferViewModel model)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var created = await _offerService.Add(model, clientId);
            return Ok(created);
        }

        /// <summary>
        /// Acepta una oferta económica y ejecuta la regla atómica de cierre en cascada (Agente).
        /// Marca la propiedad como "Vendida" y rechaza automáticamente las demás ofertas pendientes.
        /// </summary>
        [Authorize(Roles = "Agent")]
        [HttpPost("{id:int}/accept")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AcceptOfferAsync(int id)
        {
            try
            {
                await _offerService.AcceptOffer(id);
                return Ok(new { success = true, message = "Oferta aceptada. La propiedad ha sido marcada como Vendida y las demás ofertas fueron rechazadas automáticamente." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Rechaza una oferta económica individual (Agente).
        /// </summary>
        [Authorize(Roles = "Agent")]
        [HttpPost("{id:int}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RejectOfferAsync(int id)
        {
            try
            {
                await _offerService.RejectOffer(id);
                return Ok(new { success = true, message = "Oferta rechazada exitosamente." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Envía una contra-oferta al cliente con un monto y mensaje alternativo (Agente).
        /// </summary>
        [Authorize(Roles = "Agent")]
        [HttpPost("{id:int}/counter-offer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CounterOfferAsync(int id, [FromBody] CounterOfferRequest model)
        {
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(agentId)) return Unauthorized();

            if (model.CounterAmount <= 0)
            {
                return BadRequest(new { hasError = true, error = "El monto de la contra-oferta debe ser mayor a cero." });
            }

            try
            {
                await _offerService.CounterOffer(id, model.CounterAmount, model.CounterMessage, agentId);
                return Ok(new { success = true, message = "Contra-oferta enviada exitosamente al cliente." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Permite al cliente aceptar la contra-oferta enviada por el agente (Cliente).
        /// Ejecuta la regla atómica de venta.
        /// </summary>
        [Authorize(Roles = "Client")]
        [HttpPost("{id:int}/accept-counter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AcceptCounterOfferAsync(int id)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            try
            {
                await _offerService.AcceptCounterOffer(id, clientId);
                return Ok(new { success = true, message = "Contra-oferta aceptada. La propiedad ha sido marcada como Vendida." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Permite al cliente rechazar la contra-oferta del agente (Cliente).
        /// </summary>
        [Authorize(Roles = "Client")]
        [HttpPost("{id:int}/reject-counter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RejectCounterOfferAsync(int id)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            try
            {
                await _offerService.RejectCounterOffer(id, clientId);
                return Ok(new { success = true, message = "Contra-oferta rechazada." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { hasError = true, error = ex.Message });
            }
        }

        public class CounterOfferRequest
        {
            public decimal CounterAmount { get; set; }
            public string? CounterMessage { get; set; }
        }
    }
}
