using Microsoft.AspNetCore.Mvc;
using Sensore_Project.Repositories;

namespace Sensore_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly AlertsRepository _alertsRepo;

        public AlertsController(AlertsRepository alertsRepo)
        {
            _alertsRepo = alertsRepo;
        }

        // GET: api/alerts/list
        [HttpGet("list")]
        public async Task<IActionResult> GetAlerts()
        {
            var alerts = await _alertsRepo.GetAllAsync();
            return Ok(alerts);
        }

        // POST: api/alerts/resolve/5
        [HttpPost("resolve/{id}")]
        public async Task<IActionResult> ResolveAlert(int id)
        {
            await _alertsRepo.ResolveAsync(id);
            return Ok(new { message = "Alert marked as resolved" });
        }
    }
}