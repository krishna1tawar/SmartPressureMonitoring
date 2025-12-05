using Sensore_Project.Models;

namespace Sensore_Project.Repositories
{
    /// <summary>
    /// Repository interface for risk prediction operations.
    /// </summary>
    public interface IRiskPredictionRepository
    {
        Task AddAsync(RiskPrediction entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<RiskPrediction> entities, CancellationToken ct = default);
        Task<List<RiskPrediction>> GetLatestAsync(int count = 50, CancellationToken ct = default);
        Task<RiskPrediction?> GetLatestOneAsync(CancellationToken ct = default);
        Task<List<RiskPrediction>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken ct = default);

        // Map-based queries
        Task<List<RiskPrediction>> GetByAnalysisTypeAsync(string analysisType, int count = 50, CancellationToken ct = default);
        Task<List<RiskPrediction>> GetByPressureMapIdAsync(int pressureMapId, CancellationToken ct = default);
    }
}