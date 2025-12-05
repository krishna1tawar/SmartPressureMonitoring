using System;
using System.Collections.Generic;
using Sensore_Project.Models;

namespace Sensore_Project.Services
{
    /// <summary>
    /// Default implementation that uses a simple connected-components algorithm
    /// to find high-pressure regions and compute metrics over a 32x32 pressure map.
    /// </summary>
    public class PressureMapAnalysisService : IPressureMapAnalysisService
    {
        public PressureMetrics AnalyzePressureMap(PressureMap pressureMap)
        {
            if (pressureMap == null || !pressureMap.IsValid())
                throw new ArgumentException("Pressure map must be a valid 32x32 matrix.", nameof(pressureMap));

            var clusters = DetectHighPressureClusters(
                pressureMap.Matrix,
                threshold: 200,
                minClusterSize: 10,
                useEightConnectivity: true);

            int totalHighPixels = 0;
            int maxPressure = 0;
            double totalPressure = 0;
            int countedPixels = 0;

            foreach (var cluster in clusters)
            {
                totalHighPixels += cluster.PixelCount;
                maxPressure = Math.Max(maxPressure, cluster.MaxPressure);
                totalPressure += cluster.AvgPressure * cluster.PixelCount;
                countedPixels += cluster.PixelCount;
            }

            double globalAvg = countedPixels > 0 ? totalPressure / countedPixels : 0;

            // Simple risk heuristic based on coverage and intensities
            double coverage = 1024 > 0 ? (double)totalHighPixels / 1024.0 : 0.0;
            double intensityComponent = maxPressure / 255.0;
            double coverageComponent = coverage;

            double riskScore = Math.Clamp((intensityComponent * 0.6 + coverageComponent * 0.4) * 100.0, 0, 100);
            string riskLevel =
                riskScore < 25 ? "Low" :
                riskScore < 50 ? "Medium" :
                riskScore < 75 ? "High" :
                "Critical";

            return new PressureMetrics
            {
                HighPressureClusters = clusters,
                TotalHighPressurePixels = totalHighPixels,
                RiskScore = riskScore,
                RiskLevel = riskLevel,
                AlertGenerated = ShouldGenerateAlertInternal(riskScore, totalHighPixels),
                AlertTimestamp = null // to be set by caller when persisted
            };
        }

        public List<HighPressureCluster> DetectHighPressureClusters(
            int[][] matrix,
            int threshold = 200,
            int minClusterSize = 10,
            bool useEightConnectivity = true)
        {
            var clusters = new List<HighPressureCluster>();

            if (matrix == null || matrix.Length != 32)
                return clusters;

            int rows = 32;
            int cols = 32;
            var visited = new bool[rows, cols];

            // Directions: 4-connected or 8-connected
            var directions = useEightConnectivity
                ? new (int dx, int dy)[]
                {
                    (1, 0), (-1, 0), (0, 1), (0, -1),
                    (1, 1), (1, -1), (-1, 1), (-1, -1)
                }
                : new (int dx, int dy)[]
                {
                    (1, 0), (-1, 0), (0, 1), (0, -1)
                };

            int clusterId = 1;

            for (int y = 0; y < rows; y++)
            {
                var row = matrix[y];
                if (row == null || row.Length != cols)
                    continue;

                for (int x = 0; x < cols; x++)
                {
                    if (visited[y, x])
                        continue;

                    int value = row[x];
                    if (value <= threshold)
                        continue;

                    // Start BFS/DFS for this cluster
                    var queue = new Queue<(int X, int Y)>();
                    queue.Enqueue((x, y));
                    visited[y, x] = true;

                    int pixelCount = 0;
                    int maxP = 0;
                    int sumP = 0;
                    int minX = x, maxX = x, minY = y, maxY = y;
                    double sumX = 0, sumY = 0;

                    while (queue.Count > 0)
                    {
                        var (cx, cy) = queue.Dequeue();

                        int p = matrix[cy][cx];
                        pixelCount++;
                        sumP += p;
                        maxP = Math.Max(maxP, p);

                        minX = Math.Min(minX, cx);
                        maxX = Math.Max(maxX, cx);
                        minY = Math.Min(minY, cy);
                        maxY = Math.Max(maxY, cy);

                        sumX += cx;
                        sumY += cy;

                        foreach (var (dx, dy) in directions)
                        {
                            int nx = cx + dx;
                            int ny = cy + dy;

                            if (nx < 0 || nx >= cols || ny < 0 || ny >= rows)
                                continue;

                            if (visited[ny, nx])
                                continue;

                            int neighbourValue = matrix[ny][nx];
                            if (neighbourValue <= threshold)
                                continue;

                            visited[ny, nx] = true;
                            queue.Enqueue((nx, ny));
                        }
                    }

                    if (pixelCount < minClusterSize)
                        continue;

                    var cluster = new HighPressureCluster
                    {
                        ClusterId = clusterId++,
                        PixelCount = pixelCount,
                        MaxPressure = maxP,
                        AvgPressure = pixelCount > 0 ? (double)sumP / pixelCount : 0,
                        BoundingBox = new BoundingBox
                        {
                            MinX = minX,
                            MaxX = maxX,
                            MinY = minY,
                            MaxY = maxY
                        },
                        Centroid = new Point
                        {
                            X = pixelCount > 0 ? sumX / pixelCount : 0,
                            Y = pixelCount > 0 ? sumY / pixelCount : 0
                        }
                    };

                    clusters.Add(cluster);
                }
            }

            return clusters;
        }

        public bool ShouldGenerateAlert(PressureMetrics metrics)
        {
            if (metrics == null)
                return false;

            return ShouldGenerateAlertInternal(metrics.RiskScore, metrics.TotalHighPressurePixels);
        }

        private static bool ShouldGenerateAlertInternal(double riskScore, int totalHighPixels)
        {
            // Simple rule set; can be tuned later or made configurable:
            // - Any Critical risk -> alert
            // - High risk with at least moderate coverage -> alert
            // - For Medium risk, require more coverage.

            if (riskScore >= 75)
                return true;

            if (riskScore >= 50 && totalHighPixels >= 20)
                return true;

            if (riskScore >= 35 && totalHighPixels >= 60)
                return true;

            return false;
        }
    }
}
