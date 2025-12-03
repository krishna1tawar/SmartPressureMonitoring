using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sensore_Project.Models;

namespace Sensore_Project.Services
{
    public class RiskPredictionService : IRiskPredictionService
    {
        /// <summary>
        /// Existing implementation based on a single scalar pressure value.
        /// </summary>
        public Task<RiskPrediction> PredictRiskAsync(double pressure)
        {
            string level;

            // Simple threshold logic that matches your unit tests
            if (pressure < 100)
                level = "Low";
            else if (pressure < 150)
                level = "Medium";
            else
                level = "High";

            var result = new RiskPrediction
            {
                Pressure = pressure,
                RiskScore = 0,        // no ML model, always 0
                RiskLevel = level,
                Timestamp = DateTime.UtcNow,
                AnalysisType = "SingleValue"
            };

            return Task.FromResult(result);
        }

        /// <summary>
        /// Map-based risk prediction using a 32x32 pressure map and its derived metrics.
        /// </summary>
        public Task<RiskPrediction> PredictRiskFromMapAsync(PressureMap pressureMap, PressureMetrics metrics)
        {
            if (pressureMap == null)
                throw new ArgumentNullException(nameof(pressureMap));

            if (metrics == null)
                throw new ArgumentNullException(nameof(metrics));

            // Derive a single representative pressure value for backward compatibility.
            double representativePressure = ComputeRepresentativePressure(pressureMap);

            // Use metrics.RiskScore / RiskLevel as the primary signal.
            var prediction = new RiskPrediction
            {
                Pressure = representativePressure,
                RiskScore = metrics.RiskScore,
                RiskLevel = metrics.RiskLevel,
                Timestamp = DateTime.UtcNow,
                PressureMapId = null,        // to be set by caller when persisted
                AnalysisType = "PressureMap",
                MapMetrics = new MapRiskMetrics
                {
                    // These will be refined by AnalyzeRiskPatterns; initialise with defaults.
                    PatternAnalysis = new PatternAnalysis(),
                    ClusterMetrics = new ClusterMetrics
                    {
                        LargestClusterSize = 0,
                        ClusterCount = metrics.HighPressureClusters?.Count ?? 0,
                        AvgClusterPressure = 0
                    }
                }
            };

            return Task.FromResult(prediction);
        }

        /// <summary>
        /// Analyse spatial patterns of high-pressure clusters across the map.
        /// </summary>
        public MapRiskMetrics AnalyzeRiskPatterns(PressureMap pressureMap, List<HighPressureCluster> clusters)
        {
            if (pressureMap == null)
                throw new ArgumentNullException(nameof(pressureMap));

            clusters ??= new List<HighPressureCluster>();

            var metrics = new MapRiskMetrics
            {
                PatternAnalysis = new PatternAnalysis(),
                ClusterMetrics = new ClusterMetrics()
            };

            if (!pressureMap.IsValid() || clusters.Count == 0)
                return metrics;

            int largestCluster = 0;
            double totalClusterPressure = 0;
            int totalClusterPixels = 0;

            foreach (var c in clusters)
            {
                largestCluster = Math.Max(largestCluster, c.PixelCount);
                totalClusterPressure += c.AvgPressure * c.PixelCount;
                totalClusterPixels += c.PixelCount;
            }

            metrics.ClusterMetrics.LargestClusterSize = largestCluster;
            metrics.ClusterMetrics.ClusterCount = clusters.Count;
            metrics.ClusterMetrics.AvgClusterPressure = totalClusterPixels > 0
                ? totalClusterPressure / totalClusterPixels
                : 0;

            // Simple pattern analysis heuristics
            var pattern = metrics.PatternAnalysis;

            // Concentrated high pressure if one cluster dominates the pixel count.
            int totalHighPixels = totalClusterPixels;
            if (totalHighPixels > 0 && largestCluster >= totalHighPixels * 0.5)
            {
                pattern.HasConcentratedHighPressure = true;
                pattern.RiskPatterns.Add("concentrated_high");
            }

            // Distributed high pressure if many clusters with moderate size.
            if (clusters.Count >= 3 && largestCluster < totalHighPixels * 0.5)
            {
                pattern.HasDistributedHighPressure = true;
                pattern.RiskPatterns.Add("distributed_high");
            }

            // Very rough gradient estimation based on difference between top/bottom and left/right halves.
            pattern.PressureGradient = EstimateGradient(pressureMap);

            if (pattern.PressureGradient > 0.5)
            {
                pattern.RiskPatterns.Add("edge_clustering");
            }

            return metrics;
        }

        private static double ComputeRepresentativePressure(PressureMap map)
        {
            if (!map.IsValid())
                return 0;

            double sum = 0;
            int count = 0;

            for (int y = 0; y < 32; y++)
            {
                var row = map.Matrix[y];
                for (int x = 0; x < 32; x++)
                {
                    sum += row[x];
                    count++;
                }
            }

            return count > 0 ? sum / count : 0;
        }

        private static double EstimateGradient(PressureMap map)
        {
            if (!map.IsValid())
                return 0;

            int rows = 32;
            int cols = 32;

            double topSum = 0, bottomSum = 0, leftSum = 0, rightSum = 0;
            int topCount = 0, bottomCount = 0, leftCount = 0, rightCount = 0;

            for (int y = 0; y < rows; y++)
            {
                var row = map.Matrix[y];
                for (int x = 0; x < cols; x++)
                {
                    int p = row[x];

                    if (y < rows / 2)
                    {
                        topSum += p;
                        topCount++;
                    }
                    else
                    {
                        bottomSum += p;
                        bottomCount++;
                    }

                    if (x < cols / 2)
                    {
                        leftSum += p;
                        leftCount++;
                    }
                    else
                    {
                        rightSum += p;
                        rightCount++;
                    }
                }
            }

            double topAvg = topCount > 0 ? topSum / topCount : 0;
            double bottomAvg = bottomCount > 0 ? bottomSum / bottomCount : 0;
            double leftAvg = leftCount > 0 ? leftSum / leftCount : 0;
            double rightAvg = rightCount > 0 ? rightSum / rightCount : 0;

            double verticalDiff = Math.Abs(topAvg - bottomAvg) / 255.0;
            double horizontalDiff = Math.Abs(leftAvg - rightAvg) / 255.0;

            // Normalise to 0–1
            return Math.Clamp(Math.Max(verticalDiff, horizontalDiff), 0, 1);
        }
    }
}