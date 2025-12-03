using Microsoft.AspNetCore.Mvc;
using Sensore_Project.Repositories;
using Sensore_Project.Models.DTOs;

namespace Sensore_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertsRepository _alertsRepo;
        private readonly IPressureMapRepository _pressureMapRepo;

        public AlertsController(IAlertsRepository alertsRepo, IPressureMapRepository pressureMapRepo)
        {
            _alertsRepo = alertsRepo;
            _pressureMapRepo = pressureMapRepo;
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
            var alert = await _alertsRepo.GetByIdAsync(id);

            if (alert == null)
                return NotFound(new { message = "Alert not found." });

            await _alertsRepo.ResolveAsync(id);

            return Ok(new { message = "Alert marked as resolved." });
        }

        // NEW: api/alerts/by-type/{alertType}
        [HttpGet("by-type/{alertType}")]
        public async Task<IActionResult> GetAlertsByType(string alertType)
        {
            var alerts = await _alertsRepo.GetByTypeAsync(alertType);
            return Ok(alerts);
        }

        // NEW: api/alerts/by-map/{pressureMapId}
        [HttpGet("by-map/{pressureMapId:int}")]
        public async Task<IActionResult> GetAlertsByMap(int pressureMapId)
        {
            var alerts = await _alertsRepo.GetByPressureMapIdAsync(pressureMapId);
            return Ok(alerts);
        }

        // NEW: api/alerts/unresolved
        [HttpGet("unresolved")]
        public async Task<IActionResult> GetUnresolvedAlerts()
        {
            var alerts = await _alertsRepo.GetUnresolvedAsync();
            return Ok(alerts);
        }

        // NEW: api/alerts/for-review
        // Wraps flagged SensorData rows into a lightweight response for clinician dashboards.
        [HttpGet("for-review")]
        public async Task<IActionResult> GetAlertsForReview([FromQuery] int count = 100)
        {
            var flagged = await _pressureMapRepo.GetRequiringClinicianReviewAsync(count);

            var result = flagged.Select(s => new
            {
                sensorDataId = s.Id,
                timestamp = s.Timestamp,
                requiresClinicianReview = s.RequiresClinicianReview,
                metrics = s.Metrics
            });

            return Ok(result);
        }
    }
}