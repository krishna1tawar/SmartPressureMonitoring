using Microsoft.AspNetCore.Mvc;
using Sensore_Project.Repositories;
using Sensore_Project.Services;

namespace Sensore_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorDataController : ControllerBase
    {
        private readonly SensorDataRepository _sensorRepo;
        private readonly AlertsRepository _alertsRepo;
        private readonly AnomalyDetectionService _anomalyService;

        public SensorDataController(
            SensorDataRepository sensorRepo,
            AlertsRepository alertsRepo,
            AnomalyDetectionService anomalyService)
        {
            _sensorRepo = sensorRepo;
            _alertsRepo = alertsRepo;
            _anomalyService = anomalyService;
        }

        // GET: api/SensorData/latest
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest()
        {
            var latest = await _sensorRepo.GetLatestAsync();

            if (latest == null)
            {
                return NotFound(new
                {
                    message = "No sensor data found."
                });
            }

            // Check anomaly using service
            var anomaly = _anomalyService.CheckPressure(latest.Pressure);

            // If anomaly detected, create an alert
            if (anomaly.IsAnomaly)
            {
                var alert = new Models.Alert
                {
                    UserId = 1,
                    Message = "Pressure anomaly detected",
                    Pressure = latest.Pressure,
                    Timestamp = DateTime.Now,
                    IsResolved = false
                };

                await _alertsRepo.AddAsync(alert);
            }

            return Ok(new
            {
                pressure = latest.Pressure,
                timestamp = latest.Timestamp,
                anomaly = anomaly.IsAnomaly,
                score = anomaly.Score
            });
        }
    }
}