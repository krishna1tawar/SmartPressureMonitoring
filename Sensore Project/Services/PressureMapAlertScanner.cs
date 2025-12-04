using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sensore_Project.Models;
using Sensore_Project.Repositories;

namespace Sensore_Project.Services
{
    public class PressureMapAlertScanner : IPressureMapAlertScanner
    {
        private const int DefaultBatchSize = 100;

        private readonly IPressureMapRepository _pressureMapRepository;
        private readonly IAlertsRepository _alertsRepository;
        private readonly IPressureMapAnalysisService _analysisService;
        private readonly ILogger<PressureMapAlertScanner> _logger;

        public PressureMapAlertScanner(
            IPressureMapRepository pressureMapRepository,
            IAlertsRepository alertsRepository,
            IPressureMapAnalysisService analysisService,
            ILogger<PressureMapAlertScanner> logger)
        {
            _pressureMapRepository = pressureMapRepository;
            _alertsRepository = alertsRepository;
            _analysisService = analysisService;
            _logger = logger;
        }

        public async Task<ScanResult> ScanAsync(CancellationToken cancellationToken = default)
        {
            var result = new ScanResult();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var batch = await _pressureMapRepository.GetMapsNeedingAlertScanAsync(DefaultBatchSize);
                    if (batch.Count == 0)
                    {
                        result.IsComplete = true;
                        break;
                    }

                    foreach (var sensorData in batch)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            result.Cancelled = true;
                            break;
                        }

                        result.TotalMapsScanned++;

                        try
                        {
                            await ProcessSensorDataAsync(sensorData, result, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            result.Errors++;
                            var message = $"SensorData {sensorData.Id}: {ex.Message}";
                            result.ErrorMessages.Add(message);
                            _logger.LogError(ex, "Failed to scan sensor data {SensorDataId}", sensorData.Id);
                        }
                    }

                    if (result.Cancelled)
                    {
                        break;
                    }

                    if (batch.Count < DefaultBatchSize)
                    {
                        result.IsComplete = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors++;
                result.ErrorMessages.Add($"Scanner aborted: {ex.Message}");
                _logger.LogError(ex, "Pressure map alert scanner aborted due to unrecoverable error");
            }

            if (!result.Cancelled && !result.IsComplete)
            {
                result.IsComplete = true;
            }

            return result;
        }

        private async Task ProcessSensorDataAsync(SensorData sensorData, ScanResult result, CancellationToken cancellationToken)
        {
            var map = sensorData.PressureMap;
            if (map == null || !map.IsValid())
            {
                throw new InvalidOperationException("Pressure map data is missing or invalid.");
            }

            var metrics = sensorData.Metrics;
            if (metrics == null || !metrics.HasBeenScanned)
            {
                metrics = _analysisService.AnalyzePressureMap(map);
            }

            metrics.HasBeenScanned = true;

            var alertExists = await _alertsRepository.AlertExistsForMapAsync(sensorData.Id);
            if (alertExists)
            {
                result.MapsSkipped++;

                if (!metrics.AlertGenerated || metrics.AlertTimestamp == null)
                {
                    var existingAlert = (await _alertsRepository.GetByPressureMapIdAsync(sensorData.Id))
                        .OrderByDescending(a => a.Timestamp)
                        .FirstOrDefault();

                    if (existingAlert != null)
                    {
                        metrics.AlertGenerated = true;
                        metrics.AlertTimestamp = existingAlert.Timestamp;
                    }
                }

                await _pressureMapRepository.UpdateMetricsAsync(sensorData.Id, metrics);
                return;
            }

            var shouldCreateAlert = _analysisService.ShouldGenerateAlert(metrics);
            if (shouldCreateAlert)
            {
                var alert = BuildAlert(sensorData, metrics);
                await _alertsRepository.AddAsync(alert);

                metrics.AlertGenerated = true;
                metrics.AlertTimestamp = alert.Timestamp;
                result.AlertsCreated++;
            }
            else
            {
                metrics.AlertGenerated = false;
            }

            await _pressureMapRepository.UpdateMetricsAsync(sensorData.Id, metrics);
        }

        private static Alert BuildAlert(SensorData sensorData, PressureMetrics metrics)
        {
            var clusters = metrics.HighPressureClusters ?? new();
            var clusterCount = clusters.Count;
            var maxPressure = clusters.Count > 0 ? clusters.Max(c => c.MaxPressure) : (int)Math.Round(sensorData.Pressure);
            var coverage = metrics.TotalHighPressurePixels;

            var message = $"{metrics.RiskLevel} risk detected: {clusterCount} high-pressure cluster(s) found. Max pressure: {maxPressure}. Coverage: {coverage} pixels.";

            return new Alert
            {
                AlertType = "HighPressureCluster",
                Message = message,
                Pressure = maxPressure,
                Timestamp = DateTime.UtcNow,
                PressureMapId = sensorData.Id,
                ClusterInfo = new ClusterInfo
                {
                    Clusters = clusters,
                    TotalClusters = clusterCount,
                    TotalHighPressurePixels = metrics.TotalHighPressurePixels
                }
            };
        }
    }
}
