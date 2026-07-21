using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    public class AgentsController : Controller
    {
        private readonly IAgentService _agentService;

        public AgentsController(IAgentService agentService)
        {
            _agentService = agentService;
        }

        public async Task<IActionResult> Index()
        {
            var agents = await _agentService.GetAllViewModelAsync();
            // Filtrar solo agentes activos para el directorio público
            var activeAgents = agents.FindAll(a => a.IsActive);
            return View(activeAgents);
        }

        public async Task<IActionResult> Details(string id)
        {
            var agent = await _agentService.GetByIdViewModelAsync(id);

            if (agent == null)
            {
                return NotFound();
            }

            return View(agent);
        }
    }
}
