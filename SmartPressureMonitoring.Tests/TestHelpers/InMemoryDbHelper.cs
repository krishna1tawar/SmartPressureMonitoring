using Microsoft.EntityFrameworkCore;
using Sensore_Project;

namespace SmartPressureMonitoring.Tests.TestHelpers
{
    public static class InMemoryDbHelper
    {
        public static ApplicationDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}