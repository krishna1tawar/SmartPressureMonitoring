using System.Globalization;
using System.Runtime.CompilerServices;
using Sensore_Project.Models;

namespace Sensore_Project.Services
{
    /// <summary>
    /// Parses CSV files containing 32x32 pressure map matrices.
    /// Uses streaming to handle large files (~10MB) efficiently.
    /// </summary>
    public class CsvPressureMapParser : ICsvPressureMapParser
    {
        private const int MatrixSize = 32;
        private readonly ILogger<CsvPressureMapParser> _logger;

        public CsvPressureMapParser(ILogger<CsvPressureMapParser> logger)
        {
            _logger = logger;
        }

        public async IAsyncEnumerable<PressureMapBatch> ParseFileAsync(
            string filePath,
            int batchSize = 100,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError("CSV file not found: {FilePath}", filePath);
                yield break;
            }

            var fileName = Path.GetFileName(filePath);
            var (_, baseDate) = ParseFileName(fileName);

            var currentBatch = new PressureMapBatch();
            var rowBuffer = new List<int[]>();
            int mapIndex = 0;
            int lineNumber = 0;

            using var reader = new StreamReader(filePath);

            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync(cancellationToken);
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var row = ParseRow(line, lineNumber);
                if (row == null)
                    continue;

                rowBuffer.Add(row);

                // When we have 32 rows, we have a complete pressure map
                if (rowBuffer.Count == MatrixSize)
                {
                    var matrix = rowBuffer.ToArray();
                    rowBuffer.Clear();

                    var pressureMap = new PressureMap
                    {
                        Matrix = matrix,
                        Scale = "1-255",
                        Unit = "mmHg"
                    };

                    if (pressureMap.IsValid())
                    {
                        // Generate unique timestamp for each map (1 second apart)
                        var timestamp = baseDate.AddSeconds(mapIndex);

                        currentBatch.Maps.Add(pressureMap);
                        currentBatch.Timestamps.Add(timestamp);

                        if (currentBatch.StartIndex == 0 && currentBatch.Maps.Count == 1)
                        {
                            currentBatch.StartIndex = mapIndex;
                        }
                        currentBatch.EndIndex = mapIndex;

                        mapIndex++;

                        // Yield batch when full
                        if (currentBatch.Maps.Count >= batchSize)
                        {
                            yield return currentBatch;
                            currentBatch = new PressureMapBatch();
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Invalid pressure map at line {LineNumber} in {FileName}", lineNumber, fileName);
                    }
                }
            }

            // Yield remaining maps in the last batch
            if (currentBatch.Maps.Count > 0)
            {
                yield return currentBatch;
            }

            _logger.LogInformation("Parsed {MapCount} pressure maps from {FileName}", mapIndex, fileName);
        }

        public (string DeviceId, DateTime Date) ParseFileName(string fileName)
        {
            // Expected format: {deviceId}_{YYYYMMDD}.csv
            // Example: 1c0fd777_20251011.csv

            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var parts = nameWithoutExtension.Split('_');

            string deviceId = "unknown";
            DateTime date = DateTime.UtcNow.Date;

            if (parts.Length >= 2)
            {
                deviceId = parts[0];

                if (parts[1].Length == 8 &&
                    DateTime.TryParseExact(parts[1], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedDate))
                {
                    date = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                }
                else
                {
                    _logger.LogWarning("Could not parse date from filename: {FileName}, using current date", fileName);
                }
            }
            else
            {
                _logger.LogWarning("Unexpected filename format: {FileName}", fileName);
            }

            return (deviceId, date);
        }

        public async Task<int> CountMapsInFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
                return 0;

            int lineCount = 0;

            using var reader = new StreamReader(filePath);
            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(line))
                {
                    lineCount++;
                }
            }

            // Each pressure map is 32 rows
            return lineCount / MatrixSize;
        }

        private int[]? ParseRow(string line, int lineNumber)
        {
            var parts = line.Split(',');

            if (parts.Length != MatrixSize)
            {
                _logger.LogDebug("Line {LineNumber} has {ColumnCount} columns, expected {Expected}", lineNumber, parts.Length, MatrixSize);
                return null;
            }

            var row = new int[MatrixSize];

            for (int i = 0; i < MatrixSize; i++)
            {
                if (int.TryParse(parts[i].Trim(), out int value))
                {
                    // Clamp to valid range 0-255
                    row[i] = Math.Clamp(value, 0, 255);
                }
                else
                {
                    _logger.LogDebug("Non-numeric value at line {LineNumber}, column {Column}: '{Value}'", lineNumber, i, parts[i]);
                    return null;
                }
            }

            return row;
        }
    }
}
