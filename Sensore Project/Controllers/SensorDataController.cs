using Microsoft.AspNetCore.Mvc;
using Sensore_Project.Models;
using Sensore_Project.Repositories;
using Sensore_Project.Services;

namespace Sensore_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorDataController : ControllerBase
    {
        private readonly ISensorDataRepository _sensorRepo;
        private readonly IAlertsRepository _alertsRepo;
        private readonly IAnomalyDetectionService _anomalyService;

        public SensorDataController(
            ISensorDataRepository sensorRepo,
            IAlertsRepository alertsRepo,
            IAnomalyDetectionService anomalyService)
        {
            _sensorRepo = sensorRepo;
            _alertsRepo = alertsRepo;
            _anomalyService = anomalyService;
        }

        // ========================================================
        // POST: /api/sensordata/add
        // Inserts a new sensor reading
        // ========================================================
        [HttpPost("add")]
        public async Task<IActionResult> AddReading([FromBody] SensorData data)
        {
            if (data == null)
                return BadRequest(new { message = "Invalid sensor data." });

            data.Timestamp = DateTime.UtcNow;
            await _sensorRepo.AddAsync(data);

            // Run anomaly detection
            var anomaly = _anomalyService.CheckPressure(data.Pressure);
            var risk = ComputeRisk(data.Pressure, anomaly.Score);

            // Create alert if anomaly detected
            if (anomaly.IsAnomaly)
            {
                await _alertsRepo.AddAsync(new Alert
                {
                    UserId = 1,
                    Message = "Pressure anomaly detected",
                    Pressure = data.Pressure,
                    Timestamp = DateTime.Now,
                    IsResolved = false
                });
            }

            return Ok(new
            {
                message = "Sensor reading saved.",
                pressure = data.Pressure,
                timestamp = data.Timestamp,
                anomaly = anomaly.IsAnomaly,
                score = anomaly.Score,
                risk = risk.RiskLevel
            });
        }

        // ========================================================
        // GET: /api/sensordata/latest
        // Returns latest reading
        // ========================================================
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest()
        {
            var latest = await _sensorRepo.GetLatestAsync();

            if (latest == null)
                return NotFound(new { message = "No sensor data available." });

            var anomaly = _anomalyService.CheckPressure(latest.Pressure);
            var risk = ComputeRisk(latest.Pressure, anomaly.Score);

            if (anomaly.IsAnomaly)
            {
                await _alertsRepo.AddAsync(new Alert
                {
                    UserId = 1,
                    Message = "Pressure anomaly detected",
                    Pressure = latest.Pressure,
                    Timestamp = DateTime.Now,
                    IsResolved = false
                });
            }

            return Ok(new
            {
                pressure = latest.Pressure,
                timestamp = latest.Timestamp,
                anomaly = anomaly.IsAnomaly,
                score = anomaly.Score,
                riskScore = risk.RiskScore,
                riskLevel = risk.RiskLevel
            });
        }

        // ========================================================
        // GET: /api/sensordata/history?count=100
        // ========================================================
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int count = 100)
        {
            if (count <= 0) count = 50;
            if (count > 1000) count = 1000;

            var readings = await _sensorRepo.GetRecentAsync(count);

            if (readings == null || readings.Count == 0)
                return NotFound(new { message = "No sensor data found." });

            var result = readings
                .OrderBy(s => s.Timestamp)
                .Select(s =>
                {
                    var anomaly = _anomalyService.CheckPressure(s.Pressure);
                    var risk = ComputeRisk(s.Pressure, anomaly.Score);

                    return new
                    {
                        pressure = s.Pressure,
                        timestamp = s.Timestamp,
                        anomaly = anomaly.IsAnomaly,
                        score = anomaly.Score,
                        riskScore = risk.RiskScore,
                        riskLevel = risk.RiskLevel
                    };
                });

            return Ok(result);
        }

        // ========================================================
        // GET: /api/sensordata/by-date?start=...&end=...
        // ========================================================
        [HttpGet("by-date")]
        public async Task<IActionResult> GetByDate(DateTime start, DateTime end)
        {
            if (start == default || end == default)
                return BadRequest(new { message = "start and end are required." });

            if (end < start)
                return BadRequest(new { message = "end date must be >= start date." });

            var readings = await _sensorRepo.GetByDateRangeAsync(start, end);

            if (readings == null || readings.Count == 0)
                return NotFound(new { message = "No sensor data in this range." });

            var result = readings.Select(s =>
            {
                var anomaly = _anomalyService.CheckPressure(s.Pressure);
                var risk = ComputeRisk(s.Pressure, anomaly.Score);

                return new
                {
                    pressure = s.Pressure,
                    timestamp = s.Timestamp,
                    anomaly = anomaly.IsAnomaly,
                    score = anomaly.Score,
                    riskScore = risk.RiskScore,
                    riskLevel = risk.RiskLevel
                };
            });

            return Ok(result);
        }

        // ========================================================
        // RISK MODEL (0–100)
        // ========================================================
        private (double RiskScore, string RiskLevel) ComputeRisk(double pressure, double anomalyScore)
        {
            double baseRisk = anomalyScore * 70.0;

            double typical = 80.0;
            double distance = Math.Abs(pressure - typical) / typical;
            double extraRisk = distance * 30.0;

            double score = Math.Clamp(baseRisk + extraRisk, 0, 100);

            string level =
                score < 25 ? "Low" :
                score < 50 ? "Medium" :
                score < 75 ? "High" :
                "Critical";

            return (score, level);
        }
    }
}