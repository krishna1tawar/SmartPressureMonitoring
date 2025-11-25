using Sensore_Project.Services;
using Xunit;

namespace SmartPressureMonitoring.Tests.Services
{
    public class AnomalyDetectionServiceTests
    {
        [Fact]
        public void ShouldReturnNotAnomaly_ForNormalPressure()
        {
            var service = new AnomalyDetectionService();

            var result = service.CheckPressure(80);

            Assert.False(result.IsAnomaly);
            Assert.Equal(0, result.Score);
        }

        [Fact]
        public void ShouldReturnAnomaly_ForHighPressure()
        {
            var service = new AnomalyDetectionService();

            var result = service.CheckPressure(160);

            Assert.True(result.IsAnomaly);
            Assert.True(result.Score > 0);
        }

        [Fact]
        public void ShouldReturnAnomaly_ForLowPressure()
        {
            var service = new AnomalyDetectionService();

            var result = service.CheckPressure(5);

            Assert.True(result.IsAnomaly);
            Assert.True(result.Score > 0);
        }
    }
}