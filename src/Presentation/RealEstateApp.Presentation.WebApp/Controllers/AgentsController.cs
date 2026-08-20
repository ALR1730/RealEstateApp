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

        public async Task<IActionResult> Index(string? search = null, string? sort = null, bool? verified = null)
        {
            var agents = await _agentService.GetAllViewModelAsync();
            // Filtrar solo agentes activos para el directorio público
            var activeAgents = agents.FindAll(a => a.IsActive);

            if (verified == true)
            {
                activeAgents = activeAgents.FindAll(a => a.IsVerified);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                activeAgents = activeAgents.FindAll(a =>
                    (!string.IsNullOrEmpty(a.FirstName) && a.FirstName.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(a.LastName) && a.LastName.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(a.FullName) && a.FullName.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(a.Email) && a.Email.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(a.Phone) && a.Phone.ToLower().Contains(term))
                );
            }

            activeAgents = sort switch
            {
                "name_desc" => activeAgents.OrderByDescending(a => a.FullName).ToList(),
                "properties_desc" => activeAgents.OrderByDescending(a => a.PropertiesCount).ToList(),
                "properties_asc" => activeAgents.OrderBy(a => a.PropertiesCount).ToList(),
                _ => activeAgents.OrderBy(a => a.FullName).ToList()
            };

            ViewBag.SearchTerm = search;
            ViewBag.SortOrder = sort;
            ViewBag.OnlyVerified = verified;
            ViewBag.TotalCount = agents.Count(a => a.IsActive);

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
