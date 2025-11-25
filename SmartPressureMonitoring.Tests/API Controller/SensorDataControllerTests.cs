using Moq;
using Microsoft.AspNetCore.Mvc;
using Sensore_Project.Controllers;
using Sensore_Project.Repositories;
using Sensore_Project.Services;
using Sensore_Project.Models;
using Xunit;

namespace SmartPressureMonitoring.Tests.Controllers
{
    public class SensorDataControllerTests
    {
        [Fact]
        public async Task Latest_ShouldReturnNotFound_WhenNoData()
        {
            // Arrange
            var mockSensor = new Mock<ISensorDataRepository>();
            mockSensor.Setup(r => r.GetLatestAsync())
                      .ReturnsAsync((SensorData?)null);

            var mockAlerts = new Mock<IAlertsRepository>();
            var anomaly = new AnomalyDetectionService();

            var controller = new SensorDataController(
                mockSensor.Object,
                mockAlerts.Object,
                anomaly
            );

            // Act
            var response = await controller.GetLatest();

            // Assert
            Assert.IsType<NotFoundObjectResult>(response);
        }

        [Fact]
        public async Task Latest_ShouldReturnOk_WhenDataExists()
        {
            // Arrange
            var mockSensor = new Mock<ISensorDataRepository>();
            mockSensor.Setup(r => r.GetLatestAsync())
                      .ReturnsAsync(new SensorData
                      {
                          Pressure = 120,
                          Timestamp = DateTime.UtcNow
                      });

            var mockAlerts = new Mock<IAlertsRepository>();
            var anomaly = new AnomalyDetectionService();

            var controller = new SensorDataController(
                mockSensor.Object,
                mockAlerts.Object,
                anomaly
            );

            // Act
            var response = await controller.GetLatest();

            // Assert
            Assert.IsType<OkObjectResult>(response);
        }
    }
}