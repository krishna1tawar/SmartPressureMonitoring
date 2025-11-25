using Microsoft.EntityFrameworkCore;
using Sensore_Project.Models;

namespace Sensore_Project.Repositories
{
    public class SensorDataRepository : ISensorDataRepository
    {
        private readonly ApplicationDbContext _context;

        public SensorDataRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SensorData?> GetLatestAsync()
        {
            return await _context.SensorData
                .AsNoTracking()
                .OrderByDescending(s => s.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<List<SensorData>> GetRecentAsync(int count = 100)
        {
            return await _context.SensorData
                .AsNoTracking()
                .OrderByDescending(s => s.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<SensorData>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.SensorData
                .AsNoTracking()
                .Where(s => s.Timestamp >= start && s.Timestamp <= end)
                .OrderBy(s => s.Timestamp)
                .ToListAsync();
        }

        public async Task<List<SensorData>> GetByDateAsync(DateTime date)
        {
            return await _context.SensorData
                .AsNoTracking()
                .Where(s => s.Timestamp.Date == date.Date)
                .OrderBy(s => s.Timestamp)
                .ToListAsync();
        }

        public async Task AddAsync(SensorData entity)
        {
            await _context.SensorData.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<SensorData> list)
        {
            await _context.SensorData.AddRangeAsync(list);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SensorData entity)
        {
            _context.SensorData.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}