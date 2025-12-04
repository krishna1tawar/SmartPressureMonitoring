using Sensore_Project.Models;

namespace Sensore_Project.Repositories
{
    /// <summary>
    /// Repository specialised for working with SensorData rows that contain pressure maps
    /// and clinician-review flags/metrics.
    /// </summary>
    public interface IPressureMapRepository
    {
        Task<SensorData?> GetLatestAsync();
        Task<SensorData?> GetLatestWithMapAsync();
        Task<SensorData?> GetByIdAsync(int id);
        Task<List<SensorData>> GetRecentWithMapsAsync(int count = 100);
        Task<List<SensorData>> GetByDateRangeWithMapsAsync(DateTime start, DateTime end);
        Task<List<SensorData>> GetRequiringClinicianReviewAsync(int count = 100);

        Task AddAsync(SensorData entity);
        Task AddRangeAsync(IEnumerable<SensorData> entities);
        Task UpdateAsync(SensorData entity);

        /// <summary>
        /// Flags a given SensorData row for clinician review and stores pre-computed metrics.
        /// </summary>
        Task FlagForClinicianReviewAsync(int id, PressureMetrics metrics);

        /// <summary>
        /// Gets maps that need alert scanning (have PressureMapJson but no AlertGenerated flag set).
        /// </summary>
        Task<List<SensorData>> GetMapsNeedingAlertScanAsync(int batchSize = 100);

        /// <summary>
        /// Updates the metrics for a sensor data row after scanning.
        /// </summary>
        Task UpdateMetricsAsync(int id, PressureMetrics metrics);
    }
}


