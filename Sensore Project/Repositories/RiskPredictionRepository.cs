using Microsoft.EntityFrameworkCore;
using Sensore_Project.Models;

namespace Sensore_Project.Repositories
{
    public class RiskPredictionRepository : IRiskPredictionRepository
    {
        private readonly ApplicationDbContext _context;

        public RiskPredictionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RiskPrediction entity, CancellationToken ct = default)
        {
            await _context.RiskPredictions.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task AddRangeAsync(IEnumerable<RiskPrediction> entities, CancellationToken ct = default)
        {
            await _context.RiskPredictions.AddRangeAsync(entities, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<List<RiskPrediction>> GetLatestAsync(int count = 50, CancellationToken ct = default)
        {
            return await _context.RiskPredictions
                .AsNoTracking()
                .OrderByDescending(r => r.Timestamp)
                .Take(count)
                .ToListAsync(ct);
        }

        public async Task<RiskPrediction?> GetLatestOneAsync(CancellationToken ct = default)
        {
            return await _context.RiskPredictions
                .AsNoTracking()
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<List<RiskPrediction>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken ct = default)
        {
            return await _context.RiskPredictions
                .AsNoTracking()
                .Where(r => r.Timestamp >= start && r.Timestamp <= end)
                .OrderBy(r => r.Timestamp)
                .ToListAsync(ct);
        }
    }
}