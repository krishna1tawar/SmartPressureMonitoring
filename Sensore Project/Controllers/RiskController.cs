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
    }

    public class PressureRequest
    {
        public double Pressure { get; set; }
    }
}