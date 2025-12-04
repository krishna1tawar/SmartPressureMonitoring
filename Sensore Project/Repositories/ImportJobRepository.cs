using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sensore_Project.Models;

namespace Sensore_Project.Repositories
{
    /// <summary>
    /// Repository for managing import job records in the database.
    /// </summary>
    public class ImportJobRepository : IImportJobRepository
    {
        private readonly ApplicationDbContext _context;

        public ImportJobRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ImportJob?> GetLatestAsync()
        {
            return await _context.ImportJobs
                .AsNoTracking()
                .OrderByDescending(j => j.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<ImportJob?> GetByIdAsync(int id)
        {
            return await _context.ImportJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<ImportJob?> GetPendingAsync()
        {
            return await _context.ImportJobs
                .AsNoTracking()
                .Where(j => j.Status == ImportJobStatus.Pending)
                .OrderBy(j => j.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<ImportJob> CreateAsync()
        {
            var job = new ImportJob
            {
                Status = ImportJobStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ProcessedFilesList = "[]"
            };

            await _context.ImportJobs.AddAsync(job);
            await _context.SaveChangesAsync();

            return job;
        }

        public async Task UpdateStatusAsync(int id, string status)
        {
            var job = await _context.ImportJobs.FindAsync(id);
            if (job == null) return;

            job.Status = status;

            if (status == ImportJobStatus.Processing && job.StartedAt == null)
            {
                job.StartedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateProgressAsync(int id, int processedFiles, int processedMaps, string? currentFileName)
        {
            var job = await _context.ImportJobs.FindAsync(id);
            if (job == null) return;

            job.ProcessedFiles = processedFiles;
            job.ProcessedMaps = processedMaps;
            job.CurrentFileName = currentFileName;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateTotalsAsync(int id, int totalFiles, int totalMaps)
        {
            var job = await _context.ImportJobs.FindAsync(id);
            if (job == null) return;

            job.TotalFiles = totalFiles;
            job.TotalMaps = totalMaps;

            await _context.SaveChangesAsync();
        }

        public async Task MarkCompletedAsync(int id)
        {
            var job = await _context.ImportJobs.FindAsync(id);
            if (job == null) return;

            job.Status = ImportJobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
            job.CurrentFileName = null;

            await _context.SaveChangesAsync();
        }

        public async Task MarkFailedAsync(int id, string errorMessage)
        {
            var job = await _context.ImportJobs.FindAsync(id);
            if (job == null) return;

            job.Status = ImportJobStatus.Failed;
            job.CompletedAt = DateTime.UtcNow;
            job.ErrorMessage = errorMessage;
            job.CurrentFileName = null;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasPendingOrRunningJobAsync()
        {
            return await _context.ImportJobs
                .AsNoTracking()
                .AnyAsync(j => j.Status == ImportJobStatus.Pending || j.Status == ImportJobStatus.Processing);
        }

        public async Task AddProcessedFileAsync(int id, string fileName)
        {
            var job = await _context.ImportJobs.FindAsync(id);
            if (job == null) return;

            var processedFiles = string.IsNullOrEmpty(job.ProcessedFilesList)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(job.ProcessedFilesList) ?? new List<string>();

            if (!processedFiles.Contains(fileName))
            {
                processedFiles.Add(fileName);
                job.ProcessedFilesList = JsonSerializer.Serialize(processedFiles);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IReadOnlyList<ImportJob>> GetProcessingJobsAsync()
        {
            return await _context.ImportJobs
                .AsNoTracking()
                .Where(j => j.Status == ImportJobStatus.Processing)
                .ToListAsync();
        }
    }
}
