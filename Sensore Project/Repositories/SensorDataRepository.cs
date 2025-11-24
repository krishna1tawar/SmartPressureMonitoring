using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sensore_Project.Models;

namespace Sensore_Project.Repositories
{
    public class SensorDataRepository
    {
        private readonly ApplicationDbContext _context;

        public SensorDataRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns the most recent sensor reading (or null if none).
        /// </summary>
        public Task<SensorData?> GetLatestAsync()
        {
            return _context.SensorData
                .OrderByDescending(s => s.Timestamp)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns the last N readings, newest first.
        /// </summary>
        public Task<List<SensorData>> GetRecentAsync(int count)
        {
            return _context.SensorData
                .OrderByDescending(s => s.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Returns all readings between start and end (inclusive),
        /// ordered by timestamp (oldest first).
        /// </summary>
        public Task<List<SensorData>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return _context.SensorData
                .Where(s => s.Timestamp >= start && s.Timestamp <= end)
                .OrderBy(s => s.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Insert a new sensor reading.
        /// </summary>
        public async Task AddAsync(SensorData entity)
        {
            _context.SensorData.Add(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Update an existing sensor reading.
        /// </summary>
        public async Task UpdateAsync(SensorData entity)
        {
            _context.SensorData.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}