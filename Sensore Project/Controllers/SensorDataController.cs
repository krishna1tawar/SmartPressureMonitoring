using System;
using System.Linq;
using System.Threading.Tasks;
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

        // =====================================================
        // GET: api/SensorData/latest
        // Returns the most recent reading + anomaly + risk info
        // =====================================================
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

            // Check anomaly
            var anomaly = _anomalyService.CheckPressure(latest.Pressure);

            // Compute risk based on pressure + anomaly score
            var risk = ComputeRisk(latest.Pressure, anomaly.Score);

            // If anomaly detected, create an alert
            if (anomaly.IsAnomaly)
            {
                var alert = new Models.Alert
                {
                    UserId = 1, // placeholder, no auth yet
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
                score = anomaly.Score,
                riskScore = risk.RiskScore,
                riskLevel = risk.RiskLevel
            });
        }

        // =====================================================
        // GET: api/SensorData/history?count=100
        // Returns last N readings (default 100)
        // =====================================================
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int count = 100)
        {
            if (count <= 0) count = 50;
            if (count > 1000) count = 1000; // safety cap

            var readings = await _sensorRepo.GetRecentAsync(count);

            if (readings == null || readings.Count == 0)
            {
                return NotFound(new { message = "No sensor data found." });
            }

            var result = readings
                .OrderBy(s => s.Timestamp) // oldest first for charts
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

        // =====================================================
        // GET: api/SensorData/by-date?start=2025-11-01&end=2025-11-10
        // Returns readings in a date range
        // =====================================================
        [HttpGet("by-date")]
        public async Task<IActionResult> GetByDate(
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            if (start == default || end == default)
            {
                return BadRequest(new
                {
                    message = "Query parameters 'start' and 'end' are required."
                });
            }

            if (end < start)
            {
                return BadRequest(new
                {
                    message = "'end' date must be greater than or equal to 'start' date."
                });
            }

            var readings = await _sensorRepo.GetByDateRangeAsync(start, end);

            if (readings == null || readings.Count == 0)
            {
                return NotFound(new
                {
                    message = "No sensor data found in the specified date range."
                });
            }

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

        // =====================================================
        // Simple approximate risk scoring
        // Returns:
        //  - RiskScore: 0–100
        //  - RiskLevel: Low / Medium / High / Critical
        // =====================================================
        private (double RiskScore, string RiskLevel) ComputeRisk(double pressure, double anomalyScore)
        {
            // Base risk from anomaly severity (0–1 → 0–70 points)
            double baseRisk = anomalyScore * 70.0;

            // Extra risk from being far from a "typical" operating point (say 80)
            double typical = 80.0;
            double distance = Math.Abs(pressure - typical) / typical; // normalized
            double extraRisk = distance * 30.0;

            double score = baseRisk + extraRisk;

            // Clamp 0–100
            if (score < 0) score = 0;
            if (score > 100) score = 100;

            string level;
            if (score < 25) level = "Low";
            else if (score < 50) level = "Medium";
            else if (score < 75) level = "High";
            else level = "Critical";

            return (score, level);
        }
    }
}