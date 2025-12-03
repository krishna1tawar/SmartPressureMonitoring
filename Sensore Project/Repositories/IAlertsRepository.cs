using Sensore_Project.Models;

namespace Sensore_Project.Repositories
{
    public interface IAlertsRepository
    {
        Task<List<Alert>> GetAllAsync();

        Task<Alert?> GetByIdAsync(int id);

        Task AddAsync(Alert alert);
        Task ResolveAsync(int id);
        Task DeleteAsync(int id);

        // Map / type specific queries (non-breaking extensions)
        Task<List<Alert>> GetByTypeAsync(string alertType);
        Task<List<Alert>> GetByPressureMapIdAsync(int pressureMapId);
        Task<List<Alert>> GetUnresolvedAsync();
    }
}