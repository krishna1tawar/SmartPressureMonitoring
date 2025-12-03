using Microsoft.EntityFrameworkCore;
using Sensore_Project.Models;

namespace Sensore_Project.Repositories
{
    public class AlertsRepository : IAlertsRepository
    {
        private readonly ApplicationDbContext _context;

        public AlertsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Alert>> GetAllAsync()
        {
            return await _context.Alerts
                .AsNoTracking()
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<Alert?> GetByIdAsync(int id)
        {
            return await _context.Alerts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(Alert alert)
        {
            await _context.Alerts.AddAsync(alert);
            await _context.SaveChangesAsync();
        }

        public async Task ResolveAsync(int id)
        {
            var alert = await _context.Alerts.FindAsync(id);

            if (alert == null)
                return;

            alert.IsResolved = true;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var alert = await _context.Alerts.FindAsync(id);

            if (alert != null)
            {
                _context.Alerts.Remove(alert);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Alert>> GetByTypeAsync(string alertType)
        {
            if (string.IsNullOrWhiteSpace(alertType))
                return new List<Alert>();

            return await _context.Alerts
                .AsNoTracking()
                .Where(a => a.AlertType == alertType)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<List<Alert>> GetByPressureMapIdAsync(int pressureMapId)
        {
            return await _context.Alerts
                .AsNoTracking()
                .Where(a => a.PressureMapId == pressureMapId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<List<Alert>> GetUnresolvedAsync()
        {
            return await _context.Alerts
                .AsNoTracking()
                .Where(a => !a.IsResolved)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }
}