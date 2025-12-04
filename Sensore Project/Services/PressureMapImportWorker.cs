using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sensore_Project.Models;
using Sensore_Project.Repositories;

namespace Sensore_Project.Services
{
    /// <summary>
    /// Background service that processes CSV pressure map import jobs.
    /// Polls for pending jobs and processes them asynchronously.
    /// </summary>
    public class PressureMapImportWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICsvPressureMapParser _csvParser;
        private readonly ILogger<PressureMapImportWorker> _logger;
        private readonly IWebHostEnvironment _environment;

        private const int PollingIntervalSeconds = 10;
        private const int BatchSize = 100;

        public PressureMapImportWorker(
            IServiceProvider serviceProvider,
            ICsvPressureMapParser csvParser,
            ILogger<PressureMapImportWorker> logger,
            IWebHostEnvironment environment)
        {
            _serviceProvider = serviceProvider;
            _csvParser = csvParser;
            _logger = logger;
            _environment = environment;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PressureMapImportWorker started");

            // On startup, mark any stale "Processing" jobs as failed
            await MarkStaleJobsAsFailedAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingJobAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected during shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in import worker loop");
                }

                await Task.Delay(TimeSpan.FromSeconds(PollingIntervalSeconds), stoppingToken);
            }

            _logger.LogInformation("PressureMapImportWorker stopped");
        }

        private async Task MarkStaleJobsAsFailedAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var importJobRepo = scope.ServiceProvider.GetRequiredService<IImportJobRepository>();

            var staleJobs = await importJobRepo.GetProcessingJobsAsync();
            foreach (var job in staleJobs)
            {
                _logger.LogWarning("Marking stale job {JobId} as failed after restart", job.Id);
                await importJobRepo.MarkFailedAsync(job.Id, "Job did not complete before application restart.");
            }
        }

        private async Task ProcessPendingJobAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var importJobRepo = scope.ServiceProvider.GetRequiredService<IImportJobRepository>();

            // Get pending job
            var job = await importJobRepo.GetPendingAsync();
            if (job == null)
                return;

            _logger.LogInformation("Processing import job {JobId}", job.Id);

            try
            {
                await importJobRepo.UpdateStatusAsync(job.Id, ImportJobStatus.Processing);

                // Get CSV files from Data_Imports folder
                var importFolder = Path.Combine(_environment.ContentRootPath, "Data_Imports");
                if (!Directory.Exists(importFolder))
                {
                    await importJobRepo.MarkFailedAsync(job.Id, $"Import folder not found: {importFolder}");
                    return;
                }

                var csvFiles = Directory.GetFiles(importFolder, "*.csv");
                if (csvFiles.Length == 0)
                {
                    await importJobRepo.MarkFailedAsync(job.Id, "No CSV files found in Data_Imports folder");
                    return;
                }

                // Get already processed files
                var processedFiles = GetProcessedFiles(job);

                // Filter out already processed files
                var filesToProcess = csvFiles
                    .Where(f => !processedFiles.Contains(Path.GetFileName(f)))
                    .ToArray();

                if (filesToProcess.Length == 0)
                {
                    _logger.LogInformation("All files already processed for job {JobId}", job.Id);
                    await importJobRepo.MarkCompletedAsync(job.Id);
                    return;
                }

                // Count total maps across all files
                int totalMaps = 0;
                foreach (var file in filesToProcess)
                {
                    totalMaps += await _csvParser.CountMapsInFileAsync(file, stoppingToken);
                }

                await importJobRepo.UpdateTotalsAsync(job.Id, filesToProcess.Length, totalMaps);

                int processedFilesCount = 0;
                int processedMapsCount = 0;

                // Process each file
                foreach (var filePath in filesToProcess)
                {
                    stoppingToken.ThrowIfCancellationRequested();

                    var fileName = Path.GetFileName(filePath);
                    _logger.LogInformation("Processing file: {FileName}", fileName);

                    await importJobRepo.UpdateProgressAsync(
                        job.Id,
                        processedFilesCount,
                        processedMapsCount,
                        fileName);

                    using var fileScope = _serviceProvider.CreateScope();
                    var scopedProvider = fileScope.ServiceProvider;

                    // Process file in batches
                    await foreach (var batch in _csvParser.ParseFileAsync(filePath, BatchSize, stoppingToken))
                    {
                        await ProcessBatchAsync(scopedProvider, batch, stoppingToken);
                        processedMapsCount += batch.Maps.Count;

                        await importJobRepo.UpdateProgressAsync(
                            job.Id,
                            processedFilesCount,
                            processedMapsCount,
                            fileName);
                    }

                    // Mark file as processed
                    await importJobRepo.AddProcessedFileAsync(job.Id, fileName);
                    processedFilesCount++;

                    await importJobRepo.UpdateProgressAsync(
                        job.Id,
                        processedFilesCount,
                        processedMapsCount,
                        null);

                    _logger.LogInformation("Completed file: {FileName}", fileName);
                }

                await importJobRepo.MarkCompletedAsync(job.Id);
                _logger.LogInformation("Import job {JobId} completed. Processed {FileCount} files, {MapCount} maps",
                    job.Id, processedFilesCount, processedMapsCount);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Import job {JobId} was cancelled", job.Id);
                await importJobRepo.MarkFailedAsync(job.Id, "Job was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Import job {JobId} failed", job.Id);
                await importJobRepo.MarkFailedAsync(job.Id, ex.Message);
            }
        }

        private async Task ProcessBatchAsync(
            IServiceProvider serviceProvider,
            PressureMapBatch batch,
            CancellationToken stoppingToken)
        {
            var pressureMapRepo = serviceProvider.GetRequiredService<IPressureMapRepository>();

            var sensorDataList = new List<SensorData>();

            for (int i = 0; i < batch.Maps.Count; i++)
            {
                stoppingToken.ThrowIfCancellationRequested();

                var map = batch.Maps[i];
                var timestamp = batch.Timestamps[i];

                // Calculate representative pressure (average of all values)
                double avgPressure = CalculateAveragePressure(map);

                var sensorData = new SensorData
                {
                    Pressure = avgPressure,
                    Timestamp = timestamp,
                    PressureMap = map,
                    RequiresClinicianReview = false,
                    Metrics = null // Not generating alerts during import
                };

                sensorDataList.Add(sensorData);
            }

            // Batch insert
            await pressureMapRepo.AddRangeAsync(sensorDataList);
        }

        private static double CalculateAveragePressure(PressureMap map)
        {
            if (!map.IsValid())
                return 0;

            double sum = 0;
            int count = 0;

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    sum += map.Matrix[y][x];
                    count++;
                }
            }

            return count > 0 ? sum / count : 0;
        }

        private static HashSet<string> GetProcessedFiles(ImportJob job)
        {
            if (string.IsNullOrEmpty(job.ProcessedFilesList))
                return new HashSet<string>();

            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(job.ProcessedFilesList);
                return list != null ? new HashSet<string>(list) : new HashSet<string>();
            }
            catch
            {
                return new HashSet<string>();
            }
        }
    }
}
