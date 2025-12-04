using Microsoft.EntityFrameworkCore;
using System.Linq;
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

        public async Task<bool> AlertExistsForMapAsync(int pressureMapId)
        {
            return await _context.Alerts
                .AsNoTracking()
                .AnyAsync(a => a.PressureMapId == pressureMapId);
        }

        public async Task<List<Alert>> GetFilteredAsync(DateTime? start, DateTime? end, string? type, string? status)
        {
            var query = _context.Alerts.AsNoTracking().AsQueryable();

            if (start.HasValue)
                query = query.Where(a => a.Timestamp >= start.Value);

            if (end.HasValue)
                query = query.Where(a => a.Timestamp <= end.Value);

            if (!string.IsNullOrWhiteSpace(type) && type.ToLower() != "all")
                query = query.Where(a => a.AlertType == type);

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.ToLower() == "active")
                    query = query.Where(a => !a.IsResolved);
                else if (status.ToLower() == "resolved")
                    query = query.Where(a => a.IsResolved);
            }

            return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
        }

        public async Task<List<(DateTime Timestamp, int Count)>> GetTrendAsync(DateTime start, DateTime end, string bucket)
        {
            var alerts = await _context.Alerts
                .AsNoTracking()
                .Where(a => a.Timestamp >= start && a.Timestamp <= end)
                .ToListAsync();

            var grouped = bucket.ToLower() == "day"
                ? alerts.GroupBy(a => a.Timestamp.Date)
                        .Select(g => (Timestamp: g.Key, Count: g.Count()))
                        .OrderBy(x => x.Timestamp)
                        .ToList()
                : alerts.GroupBy(a => new DateTime(a.Timestamp.Year, a.Timestamp.Month, a.Timestamp.Day, a.Timestamp.Hour, 0, 0))
                        .Select(g => (Timestamp: g.Key, Count: g.Count()))
                        .OrderBy(x => x.Timestamp)
                        .ToList();

            return grouped;
        }

        public async Task<(int Total, int Active, int Resolved, double MaxPressure, Dictionary<string, int> ByType)> GetStatsAsync(DateTime? start, DateTime? end)
        {
            var query = _context.Alerts.AsNoTracking().AsQueryable();

            if (start.HasValue)
                query = query.Where(a => a.Timestamp >= start.Value);

            if (end.HasValue)
                query = query.Where(a => a.Timestamp <= end.Value);

            var alerts = await query.ToListAsync();

            var total = alerts.Count;
            var active = alerts.Count(a => !a.IsResolved);
            var resolved = alerts.Count(a => a.IsResolved);
            var maxPressure = alerts.Count > 0 ? alerts.Max(a => a.Pressure) : 0;
            var byType = alerts.GroupBy(a => a.AlertType)
                               .ToDictionary(g => g.Key, g => g.Count());

            return (total, active, resolved, maxPressure, byType);
        }
    }
}