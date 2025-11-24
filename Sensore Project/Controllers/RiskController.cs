using Microsoft.AspNetCore.Mvc;
using Sensore_Project.Services;

namespace Sensore_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RiskController : ControllerBase
    {
        private readonly RiskPredictionService _riskService;

        public RiskController(RiskPredictionService riskService)
        {
            _riskService = riskService;
        }

        /// <summary>
        /// Predict risk based on the given pressure value.
        /// </summary>
        [HttpPost("predict")]
        public async Task<IActionResult> PredictRisk([FromBody] PressureRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            var result = await _riskService.PredictRiskAsync(request.Pressure);

            if (result == null)
                return NotFound("Not enough data to train risk model.");

            return Ok(result);
        }
    }

    // Request body model
    public class PressureRequest
    {
        public double Pressure { get; set; }
    }
}