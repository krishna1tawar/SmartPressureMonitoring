using Microsoft.AspNetCore.Mvc;
using Sensore_Project.Models;
using Sensore_Project.Repositories;

namespace Sensore_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorDataController : ControllerBase
    {
        private readonly SensorDataRepository _repository;

        public SensorDataController(SensorDataRepository repository)
        {
            _repository = repository;
        }

        // ✅ GET: api/SensorData
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _repository.GetAllAsync();
            return Ok(data);
        }

        // ✅ POST: api/SensorData
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SensorData data)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            data.Timestamp = DateTime.Now;

            await _repository.AddAsync(data);
            return Ok(new { message = "Sensor data saved!" });
        }
    }
}