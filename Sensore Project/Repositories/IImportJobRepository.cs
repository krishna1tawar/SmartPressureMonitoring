using Sensore_Project.Models;

namespace Sensore_Project.Repositories
{
    /// <summary>
    /// Repository interface for managing import job records.
    /// </summary>
    public interface IImportJobRepository
    {
        Task<ImportJob?> GetLatestAsync();
        Task<ImportJob?> GetByIdAsync(int id);
        Task<ImportJob?> GetPendingAsync();
        Task<ImportJob> CreateAsync();
        Task UpdateStatusAsync(int id, string status);
        Task UpdateProgressAsync(int id, int processedFiles, int processedMaps, string? currentFileName);
        Task UpdateTotalsAsync(int id, int totalFiles, int totalMaps);
        Task MarkCompletedAsync(int id);
        Task MarkFailedAsync(int id, string errorMessage);
        Task<bool> HasPendingOrRunningJobAsync();
        Task AddProcessedFileAsync(int id, string fileName);
        Task<IReadOnlyList<ImportJob>> GetProcessingJobsAsync();
    }
}
