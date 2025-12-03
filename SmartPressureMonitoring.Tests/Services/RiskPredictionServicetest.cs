using Sensore_Project.Models;
using Sensore_Project.Services;
using Xunit;

namespace SmartPressureMonitoring.Tests.Services
{
    public class RiskPredictionServiceTests
    {
        [Fact]
        public async Task ShouldReturnLow_WhenPressureBelow100()
        {
            var service = new RiskPredictionService();

            var result = await service.PredictRiskAsync(80);

            Assert.Equal("Low", result.RiskLevel);
        }

        [Fact]
        public async Task ShouldReturnMedium_WhenPressureBetween100And150()
        {
            var service = new RiskPredictionService();

            var result = await service.PredictRiskAsync(120);

            Assert.Equal("Medium", result.RiskLevel);
        }

        [Fact]
        public async Task ShouldReturnHigh_WhenPressureAbove150()
        {
            var service = new RiskPredictionService();

            var result = await service.PredictRiskAsync(180);

            Assert.Equal("High", result.RiskLevel);
        }

        [Fact]
        public async Task PredictRiskFromMapAsync_ShouldUseMetricsRiskLevel()
        {
            var service = new RiskPredictionService();

            var map = new PressureMap
            {
                Matrix = Enumerable.Range(0, 32)
                    .Select(_ => Enumerable.Repeat(220, 32).ToArray())
                    .ToArray()
            };

            var metrics = new PressureMetrics
            {
                RiskScore = 90,
                RiskLevel = "Critical"
            };

            var result = await service.PredictRiskFromMapAsync(map, metrics);

            Assert.Equal("Critical", result.RiskLevel);
            Assert.Equal("PressureMap", result.AnalysisType);
        }
    }
}