using Sensore_Project.Models;

namespace Sensore_Project.Repositories
{
    public interface IRiskPredictionRepository
    {
        Task AddAsync(RiskPrediction entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<RiskPrediction> entities, CancellationToken ct = default);
        Task<List<RiskPrediction>> GetLatestAsync(int count = 50, CancellationToken ct = default);
        Task<RiskPrediction?> GetLatestOneAsync(CancellationToken ct = default);
        Task<List<RiskPrediction>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken ct = default);
    }
}