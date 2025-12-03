using Microsoft.AspNetCore.Mvc;
using Sensore_Project.Models;
using Sensore_Project.Models.DTOs;
using Sensore_Project.Repositories;
using Sensore_Project.Services;

namespace Sensore_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RiskController : ControllerBase
    {
        private readonly IRiskPredictionService _riskService;
        private readonly IPressureMapAnalysisService _mapAnalysisService;
        private readonly IRiskPredictionRepository _riskRepo;
        private readonly IPressureMapRepository _pressureMapRepo;

        public RiskController(
            IRiskPredictionService riskService,
            IPressureMapAnalysisService mapAnalysisService,
            IRiskPredictionRepository riskRepo,
            IPressureMapRepository pressureMapRepo)
        {
            _riskService = riskService;
            _mapAnalysisService = mapAnalysisService;
            _riskRepo = riskRepo;
            _pressureMapRepo = pressureMapRepo;
        }

        /// <summary>
        /// Predict risk score + risk level based on pressure value.
        /// </summary>
        [HttpPost("predict")]
        public async Task<IActionResult> PredictRisk([FromBody] PressureRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            var result = await _riskService.PredictRiskAsync(request.Pressure);

            return Ok(result);
        }

        /// <summary>
        /// Predict risk based on a full 32x32 pressure map.
        /// </summary>
        [HttpPost("predict-from-map")]
        public async Task<IActionResult> PredictRiskFromMap([FromBody] PressureMapRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            var map = new PressureMap
            {
                Matrix = request.Matrix,
                Scale = request.Scale ?? "1-255",
                Unit = request.Unit ?? "PSI"
            };

            if (!map.IsValid())
                return BadRequest(new { message = "Matrix must be a valid 32x32 grid." });

            // Analyse map to get cluster + risk metrics
            var metrics = _mapAnalysisService.AnalyzePressureMap(map);

            // Compute higher-level risk patterns
            var mapRiskMetrics = _riskService.AnalyzeRiskPatterns(map, metrics.HighPressureClusters);

            // Build prediction entity
            var prediction = await _riskService.PredictRiskFromMapAsync(map, metrics);
            prediction.MapMetrics = mapRiskMetrics;

            // Persist both SensorData row and RiskPrediction in one go
            var sensorRow = new SensorData
            {
                Pressure = prediction.Pressure,
                PressureMap = map,
                Metrics = metrics,
                RequiresClinicianReview = metrics.AlertGenerated,
                Timestamp = DateTime.UtcNow
            };

            await _pressureMapRepo.AddAsync(sensorRow);

            // Link prediction to stored map row
            prediction.PressureMapId = sensorRow.Id;
            await _riskRepo.AddAsync(prediction);

            var response = new RiskPredictionResponse
            {
                Id = prediction.Id,
                RiskScore = prediction.RiskScore,
                RiskLevel = prediction.RiskLevel,
                AnalysisType = prediction.AnalysisType,
                PressureMapId = prediction.PressureMapId,
                Timestamp = prediction.Timestamp,
                MapMetrics = prediction.MapMetrics
            };

            return Ok(response);
        }

        /// <summary>
        /// Get latest map-based risk predictions.
        /// </summary>
        [HttpGet("latest-maps")]
        public async Task<IActionResult> GetLatestMapPredictions([FromQuery] int count = 50)
        {
            var list = await _riskRepo.GetByAnalysisTypeAsync("PressureMap", count);

            var result = list.Select(r => new RiskPredictionResponse
            {
                Id = r.Id,
                RiskScore = r.RiskScore,
                RiskLevel = r.RiskLevel,
                AnalysisType = r.AnalysisType,
                PressureMapId = r.PressureMapId,
                Timestamp = r.Timestamp,
                MapMetrics = r.MapMetrics
            });

            return Ok(result);
        }

        /// <summary>
        /// Get risk predictions filtered by analysis type (SingleValue or PressureMap).
        /// </summary>
        [HttpGet("by-type/{analysisType}")]
        public async Task<IActionResult> GetPredictionsByType(string analysisType, [FromQuery] int count = 50)
        {
            var list = await _riskRepo.GetByAnalysisTypeAsync(analysisType, count);

            var result = list.Select(r => new RiskPredictionResponse
            {
                Id = r.Id,
                RiskScore = r.RiskScore,
                RiskLevel = r.RiskLevel,
                AnalysisType = r.AnalysisType,
                PressureMapId = r.PressureMapId,
                Timestamp = r.Timestamp,
                MapMetrics = r.MapMetrics
            });

            return Ok(result);
        }

        /// <summary>
        /// Get all risk predictions linked to a specific pressure map row.
        /// </summary>
        [HttpGet("by-map/{pressureMapId:int}")]
        public async Task<IActionResult> GetPredictionsByMap(int pressureMapId)
        {
            var list = await _riskRepo.GetByPressureMapIdAsync(pressureMapId);

            var result = list.Select(r => new RiskPredictionResponse
            {
                Id = r.Id,
                RiskScore = r.RiskScore,
                RiskLevel = r.RiskLevel,
                AnalysisType = r.AnalysisType,
                PressureMapId = r.PressureMapId,
                Timestamp = r.Timestamp,
                MapMetrics = r.MapMetrics
            });

            return Ok(result);
        }
    }

    public class PressureRequest
    {
        public double Pressure { get; set; }
    }
}