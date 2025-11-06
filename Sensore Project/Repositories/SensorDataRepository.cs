using Sensore_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Sensore_Project.Repositories
{
    public class SensorDataRepository
    {
        private readonly ApplicationDbContext _context;

        public SensorDataRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SensorData>> GetAllAsync()
        {
            return await _context.SensorData.ToListAsync();
        }

        public async Task AddAsync(SensorData data)
        {
            _context.SensorData.Add(data);
            await _context.SaveChangesAsync();
        }
    }
}