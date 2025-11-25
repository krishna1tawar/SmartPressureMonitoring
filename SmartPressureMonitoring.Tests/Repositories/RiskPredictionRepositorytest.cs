using Microsoft.EntityFrameworkCore;
using Sensore_Project.Models;
using Sensore_Project.Repositories;
using SmartPressureMonitoring.Tests.TestHelpers;
using Xunit;
using System.Threading.Tasks;

namespace SmartPressureMonitoring.Tests.Repositories
{
    public class RiskPredictionRepositoryTests
    {
        [Fact]
        public async Task ShouldAddPrediction()
        {
            var db = InMemoryDbHelper.CreateDb();
            var repo = new RiskPredictionRepository(db);

            await repo.AddAsync(new RiskPrediction
            {
                Pressure = 120,
                RiskScore = 0.8,
                RiskLevel = "High",
                Timestamp = DateTime.UtcNow
            });

            Assert.Equal(1, await db.RiskPredictions.CountAsync());
        }

        [Fact]
        public async Task ShouldReturnLatestPrediction()
        {
            var db = InMemoryDbHelper.CreateDb();
            var repo = new RiskPredictionRepository(db);

            await repo.AddAsync(new RiskPrediction
            {
                Pressure = 80,
                RiskScore = 0.3,
                RiskLevel = "Medium",
                Timestamp = DateTime.UtcNow.AddMinutes(-10)
            });

            await repo.AddAsync(new RiskPrediction
            {
                Pressure = 140,
                RiskScore = 0.9,
                RiskLevel = "High",
                Timestamp = DateTime.UtcNow
            });

            var latest = await repo.GetLatestOneAsync();

            Assert.NotNull(latest);
            Assert.Equal(140, latest!.Pressure);
        }
    }
}