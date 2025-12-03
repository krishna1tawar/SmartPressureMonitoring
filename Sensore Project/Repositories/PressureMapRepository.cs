using Microsoft.EntityFrameworkCore;
using Sensore_Project.Models;

namespace Sensore_Project.Repositories
{
    /// <summary>
    /// Repository for working with SensorData rows that include pressure-map JSON,
    /// clinician-review flags, and metrics JSON.
    /// </summary>
    public class PressureMapRepository : IPressureMapRepository
    {
        private readonly ApplicationDbContext _context;

        public PressureMapRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SensorData?> GetLatestWithMapAsync()
        {
            return await _context.SensorData
                .AsNoTracking()
                .Where(s => s.PressureMapJson != null)
                .OrderByDescending(s => s.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<List<SensorData>> GetRecentWithMapsAsync(int count = 100)
        {
            if (count <= 0) count = 50;
            if (count > 1000) count = 1000;

            return await _context.SensorData
                .AsNoTracking()
                .Where(s => s.PressureMapJson != null)
                .OrderByDescending(s => s.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<SensorData>> GetByDateRangeWithMapsAsync(DateTime start, DateTime end)
        {
            return await _context.SensorData
                .AsNoTracking()
                .Where(s => s.PressureMapJson != null &&
                            s.Timestamp >= start &&
                            s.Timestamp <= end)
                .OrderBy(s => s.Timestamp)
                .ToListAsync();
        }

        public async Task<List<SensorData>> GetRequiringClinicianReviewAsync(int count = 100)
        {
            if (count <= 0) count = 50;
            if (count > 1000) count = 1000;

            return await _context.SensorData
                .AsNoTracking()
                .Where(s => s.RequiresClinicianReview)
                .OrderByDescending(s => s.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task AddAsync(SensorData entity)
        {
            await _context.SensorData.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<SensorData> entities)
        {
            await _context.SensorData.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SensorData entity)
        {
            _context.SensorData.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task FlagForClinicianReviewAsync(int id, PressureMetrics metrics)
        {
            var row = await _context.SensorData.FindAsync(id);
            if (row == null)
                return;

            row.RequiresClinicianReview = true;
            row.Metrics = metrics;

            await _context.SaveChangesAsync();
        }
    }
}


