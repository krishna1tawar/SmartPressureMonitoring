using Microsoft.AspNetCore.Mvc;
using Sensore_Project.Models;
using Sensore_Project.Repositories;

namespace Sensore_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly IImportJobRepository _importJobRepo;

        public ImportController(IImportJobRepository importJobRepo)
        {
            _importJobRepo = importJobRepo;
        }

        /// <summary>
        /// Starts a new pressure map import job if none are currently pending or processing.
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> StartImportAsync()
        {
            if (await _importJobRepo.HasPendingOrRunningJobAsync())
            {
                var existing = await _importJobRepo.GetLatestAsync();
                return Conflict(new
                {
                    message = "An import job is already pending or in progress.",
                    jobId = existing?.Id
                });
            }

            var job = await _importJobRepo.CreateAsync();
            return Ok(ToResponse(job));
        }

        /// <summary>
        /// Gets the status of the latest import job.
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetLatestStatusAsync()
        {
            var job = await _importJobRepo.GetLatestAsync();
            if (job == null)
                return NotFound(new { message = "No import jobs found." });

            return Ok(ToResponse(job));
        }

        /// <summary>
        /// Gets the status of a specific import job by ID.
        /// </summary>
        [HttpGet("status/{id:int}")]
        public async Task<IActionResult> GetStatusByIdAsync(int id)
        {
            var job = await _importJobRepo.GetByIdAsync(id);
            if (job == null)
                return NotFound(new { message = "Import job not found." });

            return Ok(ToResponse(job));
        }

        private static ImportStatusResponse ToResponse(ImportJob job) => new()
        {
            Id = job.Id,
            Status = job.Status,
            TotalFiles = job.TotalFiles,
            ProcessedFiles = job.ProcessedFiles,
            TotalMaps = job.TotalMaps,
            ProcessedMaps = job.ProcessedMaps,
            CurrentFileName = job.CurrentFileName,
            ErrorMessage = job.ErrorMessage,
            CreatedAt = job.CreatedAt,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt
        };

        public class ImportStatusResponse
        {
            public int Id { get; set; }
            public string Status { get; set; } = string.Empty;
            public int TotalFiles { get; set; }
            public int ProcessedFiles { get; set; }
            public int TotalMaps { get; set; }
            public int ProcessedMaps { get; set; }
            public string? CurrentFileName { get; set; }
            public string? ErrorMessage { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? StartedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
        }
    }
}
