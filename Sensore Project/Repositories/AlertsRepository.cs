using Sensore_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Sensore_Project.Repositories
{
    public class AlertsRepository
    {
        private readonly ApplicationDbContext _context;

        public AlertsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Alert>> GetAllAsync()
        {
            return await _context.Alerts
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task AddAsync(Alert alert)
        {
            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();
        }

        public async Task ResolveAsync(int id)
        {
            var alert = await _context.Alerts.FindAsync(id);
            if (alert == null) return;

            alert.IsResolved = true;
            await _context.SaveChangesAsync();
        }
    }
}