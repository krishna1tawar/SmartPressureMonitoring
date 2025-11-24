using Microsoft.AspNetCore.Mvc;
using Sensore_Project.Services;

namespace Sensore_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnomalyController : ControllerBase
    {
        private readonly AnomalyDetectionService _anomalyService;

        public AnomalyController(AnomalyDetectionService anomalyService)
        {
            _anomalyService = anomalyService;
        }

        /// <summary>
        /// Detect anomalies from a single pressure value.
        /// </summary>
        [HttpPost("check")]
        public IActionResult Check([FromBody] AnomalyRequest req)
        {
            var result = _anomalyService.CheckPressure(req.Pressure);

            return Ok(new
            {
                isAnomaly = result.IsAnomaly,
                score = result.Score
            });
        }
    }

    public class AnomalyRequest
    {
        public double Pressure { get; set; }
    }
}