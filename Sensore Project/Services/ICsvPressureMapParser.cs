using Sensore_Project.Models;

namespace Sensore_Project.Services
{
    /// <summary>
    /// Service interface for parsing CSV files containing pressure map data.
    /// </summary>
    public interface ICsvPressureMapParser
    {
        /// <summary>
        /// Parses a CSV file and yields pressure maps in batches.
        /// Uses streaming to handle large files efficiently.
        /// </summary>
        IAsyncEnumerable<PressureMapBatch> ParseFileAsync(
            string filePath,
            int batchSize = 100,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Extracts device ID and date from the filename pattern: {deviceId}_{YYYYMMDD}.csv
        /// </summary>
        (string DeviceId, DateTime Date) ParseFileName(string fileName);

        /// <summary>
        /// Counts total pressure maps in a file without loading them all into memory.
        /// </summary>
        Task<int> CountMapsInFileAsync(string filePath, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents a batch of parsed pressure maps.
    /// </summary>
    public class PressureMapBatch
    {
        public List<PressureMap> Maps { get; set; } = new();
        public List<DateTime> Timestamps { get; set; } = new();
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }
}
