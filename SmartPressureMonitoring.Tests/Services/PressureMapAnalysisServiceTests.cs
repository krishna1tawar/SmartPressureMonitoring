using Sensore_Project.Models;
using Sensore_Project.Services;
using Xunit;

namespace SmartPressureMonitoring.Tests.Services
{
    public class PressureMapAnalysisServiceTests
    {
        private static PressureMap CreateMapWithCluster(int value, int size)
        {
            var matrix = new int[32][];
            for (int y = 0; y < 32; y++)
            {
                matrix[y] = new int[32];
            }

            // Create a simple contiguous block from (0,0) of the requested size (row-major)
            int placed = 0;
            for (int y = 0; y < 32 && placed < size; y++)
            {
                for (int x = 0; x < 32 && placed < size; x++)
                {
                    matrix[y][x] = value;
                    placed++;
                }
            }

            return new PressureMap { Matrix = matrix };
        }

        [Fact]
        public void AnalyzePressureMap_ShouldDetectCluster_WhenAboveThreshold()
        {
            var service = new PressureMapAnalysisService();
            var map = CreateMapWithCluster(220, 15); // > threshold (200), size >= 10

            var metrics = service.AnalyzePressureMap(map);

            Assert.NotNull(metrics);
            Assert.NotEmpty(metrics.HighPressureClusters);
            Assert.True(metrics.TotalHighPressurePixels >= 15);
        }

        [Fact]
        public void DetectHighPressureClusters_ShouldRespectMinClusterSize()
        {
            var service = new PressureMapAnalysisService();
            var map = CreateMapWithCluster(220, 5); // below minClusterSize (10)

            var clusters = service.DetectHighPressureClusters(map.Matrix, threshold: 200, minClusterSize: 10);

            Assert.Empty(clusters);
        }

        [Fact]
        public void ShouldGenerateAlert_WhenRiskScoreHigh()
        {
            var service = new PressureMapAnalysisService();
            var metrics = new PressureMetrics
            {
                RiskScore = 80,
                TotalHighPressurePixels = 30
            };

            var shouldAlert = service.ShouldGenerateAlert(metrics);

            Assert.True(shouldAlert);
        }
    }
}


