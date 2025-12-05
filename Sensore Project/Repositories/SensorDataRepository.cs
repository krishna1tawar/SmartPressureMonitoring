using Microsoft.EntityFrameworkCore;
using Sensore_Project.Models;

namespace Sensore_Project.Repositories
{
    /// <summary>
    /// Repository for managing sensor data records in the database.
    /// </summary>
    public class SensorDataRepository : ISensorDataRepository
    {
        private readonly ApplicationDbContext _context;

        public SensorDataRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the most recent sensor data record.
        /// </summary>
        public async Task<SensorData?> GetLatestAsync()
        {
            return await _context.SensorData
                .AsNoTracking()
                .OrderByDescending(s => s.Timestamp)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets the most recent sensor data records up to the specified count.
        /// </summary>
        public async Task<List<SensorData>> GetRecentAsync(int count = 100)
        {
            return await _context.SensorData
                .AsNoTracking()
                .OrderByDescending(s => s.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Gets sensor data records within a date range.
        /// </summary>
        public async Task<List<SensorData>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.SensorData
                .AsNoTracking()
                .Where(s => s.Timestamp >= start && s.Timestamp <= end)
                .OrderBy(s => s.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Gets sensor data records for a specific date.
        /// </summary>
        public async Task<List<SensorData>> GetByDateAsync(DateTime date)
        {
            return await _context.SensorData
                .AsNoTracking()
                .Where(s => s.Timestamp.Date == date.Date)
                .OrderBy(s => s.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new sensor data record.
        /// </summary>
        public async Task AddAsync(SensorData entity)
        {
            await _context.SensorData.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Adds multiple sensor data records in a batch.
        /// </summary>
        public async Task AddRangeAsync(IEnumerable<SensorData> list)
        {
            await _context.SensorData.AddRangeAsync(list);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing sensor data record.
        /// </summary>
        public async Task UpdateAsync(SensorData entity)
        {
            _context.SensorData.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}