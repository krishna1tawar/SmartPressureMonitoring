using Microsoft.EntityFrameworkCore;
using Sensore_Project.Models;
using Sensore_Project.Repositories;
using SmartPressureMonitoring.Tests.TestHelpers;
using System.Threading.Tasks;
using Xunit;

namespace SmartPressureMonitoring.Tests.Repositories
{
    public class AlertsRepositoryTests
    {
        [Fact]
        public async Task ShouldAddAlert()
        {
            var db = InMemoryDbHelper.CreateDb();
            var repo = new AlertsRepository(db);

            await repo.AddAsync(new Alert
            {
                Pressure = 150,
                Timestamp = DateTime.UtcNow,
                Message = "Test Alert",
                IsResolved = false,
                UserId = 1
            });

            Assert.Equal(1, await db.Alerts.CountAsync());
        }

        [Fact]
        public async Task ShouldResolveAlert()
        {
            var db = InMemoryDbHelper.CreateDb();
            var repo = new AlertsRepository(db);

            var alert = new Alert
            {
                Pressure = 150,
                Timestamp = DateTime.UtcNow,
                Message = "Test Resolve",
                IsResolved = false,
                UserId = 1
            };

            await repo.AddAsync(alert);

            await repo.ResolveAsync(alert.Id);   // ✔ Correct method name

            var updated = await db.Alerts.FirstAsync();
            Assert.True(updated.IsResolved);
        }
    }
}