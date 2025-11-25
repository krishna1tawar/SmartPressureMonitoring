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
    }
}