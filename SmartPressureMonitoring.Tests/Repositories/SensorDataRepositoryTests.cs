using Microsoft.EntityFrameworkCore;
using Sensore_Project.Models;
using Sensore_Project.Repositories;
using SmartPressureMonitoring.Tests.TestHelpers;
using System.Threading.Tasks;
using Xunit;

namespace SmartPressureMonitoring.Tests.Repositories
{
    public class SensorDataRepositoryTests
    {
        [Fact]
        public async Task ShouldSaveSensorReading()
        {
            var db = InMemoryDbHelper.CreateDb();
            var repo = new SensorDataRepository(db);

            await repo.AddAsync(new SensorData
            {
                Pressure = 120,
                Timestamp = DateTime.UtcNow
            });

            Assert.Equal(1, await db.SensorData.CountAsync());
        }

        [Fact]
        public async Task ShouldReturnLatestReading()
        {
            var db = InMemoryDbHelper.CreateDb();
            var repo = new SensorDataRepository(db);

            await repo.AddAsync(new SensorData
            {
                Pressure = 90,
                Timestamp = DateTime.UtcNow.AddMinutes(-5)
            });

            await repo.AddAsync(new SensorData
            {
                Pressure = 150,
                Timestamp = DateTime.UtcNow
            });

            var latest = await repo.GetLatestAsync();

            Assert.NotNull(latest);
            Assert.Equal(150, latest!.Pressure);
        }
    }
}