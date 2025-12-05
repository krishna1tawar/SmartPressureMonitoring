using Microsoft.EntityFrameworkCore;
using Sensore_Project.Models;

namespace Sensore_Project.Repositories
{
    /// <summary>
    /// Repository for managing risk prediction records in the database.
    /// </summary>
    public class RiskPredictionRepository : IRiskPredictionRepository
    {
        private readonly ApplicationDbContext _context;

        public RiskPredictionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new risk prediction record.
        /// </summary>
        public async Task AddAsync(RiskPrediction entity, CancellationToken ct = default)
        {
            await _context.RiskPredictions.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Adds multiple risk prediction records in a batch.
        /// </summary>
        public async Task AddRangeAsync(IEnumerable<RiskPrediction> entities, CancellationToken ct = default)
        {
            await _context.RiskPredictions.AddRangeAsync(entities, ct);
            await _context.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Gets the most recent risk predictions up to the specified count.
        /// </summary>
        public async Task<List<RiskPrediction>> GetLatestAsync(int count = 50, CancellationToken ct = default)
        {
            return await _context.RiskPredictions
                .AsNoTracking()
                .OrderByDescending(r => r.Timestamp)
                .Take(count)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Gets the single most recent risk prediction.
        /// </summary>
        public async Task<RiskPrediction?> GetLatestOneAsync(CancellationToken ct = default)
        {
            return await _context.RiskPredictions
                .AsNoTracking()
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync(ct);
        }

        /// <summary>
        /// Gets risk predictions within a date range.
        /// </summary>
        public async Task<List<RiskPrediction>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken ct = default)
        {
            return await _context.RiskPredictions
                .AsNoTracking()
                .Where(r => r.Timestamp >= start && r.Timestamp <= end)
                .OrderBy(r => r.Timestamp)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Gets risk predictions filtered by analysis type (SingleValue or PressureMap).
        /// </summary>
        public async Task<List<RiskPrediction>> GetByAnalysisTypeAsync(string analysisType, int count = 50, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(analysisType))
                return new List<RiskPrediction>();

            if (count <= 0) count = 50;
            if (count > 1000) count = 1000;

            return await _context.RiskPredictions
                .AsNoTracking()
                .Where(r => r.AnalysisType == analysisType)
                .OrderByDescending(r => r.Timestamp)
                .Take(count)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Gets all risk predictions linked to a specific pressure map.
        /// </summary>
        public async Task<List<RiskPrediction>> GetByPressureMapIdAsync(int pressureMapId, CancellationToken ct = default)
        {
            return await _context.RiskPredictions
                .AsNoTracking()
                .Where(r => r.PressureMapId == pressureMapId)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync(ct);
        }
    }
}