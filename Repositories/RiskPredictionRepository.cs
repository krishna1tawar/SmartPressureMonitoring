using Microsoft.EntityFrameworkCore;
using Sensore_Project.Models;

namespace Sensore_Project.Repositories
{
    public class RiskPredictionRepository
    {
        private readonly ApplicationDbContext _context;

        public RiskPredictionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Save a single risk prediction row.
        /// </summary>
        public async Task AddAsync(RiskPrediction entity, CancellationToken ct = default)
        {
            _context.RiskPredictions.Add(entity);
            await _context.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Save a batch of risk predictions.
        /// </summary>
        public async Task AddRangeAsync(IEnumerable<RiskPrediction> entities, CancellationToken ct = default)
        {
            await _context.RiskPredictions.AddRangeAsync(entities, ct);
            await _context.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Get latest N risk predictions (for dashboard).
        /// </summary>
        public async Task<List<RiskPrediction>> GetLatestAsync(int count = 50, CancellationToken ct = default)
        {
            return await _context.RiskPredictions
                .OrderByDescending(r => r.Timestamp)
                .Take(count)
                .AsNoTracking()
                .ToListAsync(ct);
        }
    }
}