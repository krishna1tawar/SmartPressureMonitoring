using System;
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
            // Fetching all alerts
            return await _context.Alerts
                .AsNoTracking()
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<Alert?> GetByIdAsync(int id)
        {
            // Fetching alert by id with comments
            return await _context.Alerts
                .AsNoTracking()
                .Include(a => a.Comments)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(Alert alert)
        {
            // Adding new alert
            await _context.Alerts.AddAsync(alert);
            await _context.SaveChangesAsync();
        }

        public async Task ResolveAsync(int id)
        {
            // Resolving alert
            var alert = await _context.Alerts.FindAsync(id);

            if (alert == null)
                return;

            alert.IsResolved = true;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            // Deleting alert
            var alert = await _context.Alerts.FindAsync(id);

            if (alert != null)
            {
                _context.Alerts.Remove(alert);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Alert>> GetByTypeAsync(string alertType)
        {
            // Fetching alerts by type
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
            // Fetching alerts by pressure map id
            return await _context.Alerts
                .AsNoTracking()
                .Where(a => a.PressureMapId == pressureMapId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<List<Alert>> GetUnresolvedAsync()
        {
            // Fetching unresolved alerts
            return await _context.Alerts
                .AsNoTracking()
                .Where(a => !a.IsResolved)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<bool> AlertExistsForMapAsync(int pressureMapId)
        {
            // Checking if alert exists for map
            return await _context.Alerts
                .AsNoTracking()
                .AnyAsync(a => a.PressureMapId == pressureMapId);
        }

        public async Task<List<Alert>> GetFilteredAsync(DateTime? start, DateTime? end, string? type, string? status)
        {
            // Fetching filtered alerts
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
            // Fetching trend
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
            // Fetching stats
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

        public async Task<List<Comment>> GetCommentsForAlertAsync(int alertId)
        {
            // Fetching comments for alert
            return await _context.Comments
                .AsNoTracking()
                .Where(c => c.AlertId == alertId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment?> AddCommentAsync(int alertId, int userId, string commentText)
        {
            // Adding comment
            var alertExists = await _context.Alerts.AnyAsync(a => a.Id == alertId);
            if (!alertExists)
            {
                return null;
            }

            var comment = new Comment
            {
                AlertId = alertId,
                UserId = userId,
                CommentText = commentText,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return comment;
        }

        public async Task<Comment?> AddOrUpdateFeedbackAsync(int commentId, int feedbackUserId, string feedbackText)
        {
            // Adding or updating feedback
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
            {
                return null;
            }

            comment.FeedbackText = feedbackText;
            comment.FeedbackUserId = feedbackUserId;
            comment.FeedbackProvidedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return comment;
        }
    }
}