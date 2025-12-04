using Microsoft.AspNetCore.Mvc;
using Sensore_Project.Models;
using Sensore_Project.Repositories;
using Sensore_Project.Models.DTOs;
using Sensore_Project.Services;

namespace Sensore_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertsRepository _alertsRepo;
        private readonly IPressureMapRepository _pressureMapRepo;
        private readonly IPressureMapAlertScanner _scanner;

        public AlertsController(
            IAlertsRepository alertsRepo,
            IPressureMapRepository pressureMapRepo,
            IPressureMapAlertScanner scanner)
        {
            _alertsRepo = alertsRepo;
            _pressureMapRepo = pressureMapRepo;
            _scanner = scanner;
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

        // GET: api/alerts/filtered?start=...&end=...&type=...&status=...
        [HttpGet("filtered")]
        public async Task<IActionResult> GetFilteredAlerts(
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end,
            [FromQuery] string? type,
            [FromQuery] string? status)
        {
            var alerts = await _alertsRepo.GetFilteredAsync(start, end, type, status);
            return Ok(alerts);
        }

        // GET: api/alerts/trend?start=...&end=...&bucket=hour|day
        [HttpGet("trend")]
        public async Task<IActionResult> GetAlertTrend(
            [FromQuery] DateTime start,
            [FromQuery] DateTime end,
            [FromQuery] string bucket = "hour")
        {
            var trendData = await _alertsRepo.GetTrendAsync(start, end, bucket);

            var response = new AlertTrendResponse
            {
                Bucket = bucket,
                DataPoints = trendData.Select(t => new AlertTrendPoint
                {
                    Timestamp = t.Timestamp,
                    Count = t.Count
                }).ToList()
            };

            return Ok(response);
        }

        // GET: api/alerts/stats?start=...&end=...
        [HttpGet("stats")]
        public async Task<IActionResult> GetAlertStats(
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end)
        {
            var (total, active, resolved, maxPressure, byType) = await _alertsRepo.GetStatsAsync(start, end);

            var response = new AlertStatsResponse
            {
                Total = total,
                Active = active,
                Resolved = resolved,
                MaxPressure = maxPressure,
                ByType = byType
            };

            return Ok(response);
        }

        // GET: api/alerts/{id}/detail
        [HttpGet("{id:int}/detail")]
        public async Task<IActionResult> GetAlertDetail(int id)
        {
            var alert = await _alertsRepo.GetByIdAsync(id);
            if (alert == null)
                return NotFound(new { message = "Alert not found." });

            var response = new AlertDetailResponse
            {
                Id = alert.Id,
                AlertType = alert.AlertType,
                Message = alert.Message,
                Pressure = alert.Pressure,
                Timestamp = alert.Timestamp,
                IsResolved = alert.IsResolved,
                PressureMapId = alert.PressureMapId,
                ClusterInfo = alert.ClusterInfo
            };

            // Fetch pressure map if available
            if (alert.PressureMapId.HasValue)
            {
                var sensorData = await _pressureMapRepo.GetLatestAsync();
                // Try to find the specific sensor data by ID
                var mapData = await _pressureMapRepo.GetByIdAsync(alert.PressureMapId.Value);
                if (mapData?.PressureMap != null)
                {
                    response.PressureMapMatrix = mapData.PressureMap.Matrix;
                    response.MapScale = mapData.PressureMap.Scale;
                    response.MapUnit = mapData.PressureMap.Unit;
                    response.Metrics = mapData.Metrics;
                }
            }

            return Ok(response);
        }

        // POST: api/alerts/scan
        [HttpPost("scan")]
        public async Task<IActionResult> ScanForAlerts(CancellationToken cancellationToken)
        {
            var scanResult = await _scanner.ScanAsync(cancellationToken);

            var response = new ScanResultResponse
            {
                TotalMapsScanned = scanResult.TotalMapsScanned,
                AlertsCreated = scanResult.AlertsCreated,
                MapsSkipped = scanResult.MapsSkipped,
                Errors = scanResult.Errors,
                ErrorMessages = scanResult.ErrorMessages,
                IsComplete = scanResult.IsComplete,
                Cancelled = scanResult.Cancelled,
                Message = $"Scan complete: {scanResult.AlertsCreated} alert(s) created, {scanResult.MapsSkipped} map(s) skipped, {scanResult.Errors} error(s)."
            };

            return Ok(response);
        }
    }
}