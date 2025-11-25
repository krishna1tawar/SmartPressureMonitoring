using Sensore_Project.Models;

namespace Sensore_Project.Repositories
{
    public interface ISensorDataRepository
    {
        Task<SensorData?> GetLatestAsync();
        Task<List<SensorData>> GetRecentAsync(int count = 100);
        Task<List<SensorData>> GetByDateRangeAsync(DateTime start, DateTime end);
        Task<List<SensorData>> GetByDateAsync(DateTime date);
        Task AddAsync(SensorData entity);
        Task AddRangeAsync(IEnumerable<SensorData> list);
        Task UpdateAsync(SensorData entity);
    }
}