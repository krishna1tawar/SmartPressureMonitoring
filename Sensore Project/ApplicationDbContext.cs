using Microsoft.EntityFrameworkCore;
using Sensore_Project.Models;

namespace Sensore_Project
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SensorData> SensorData { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<RiskPrediction> RiskPredictions { get; set; }
        public DbSet<ImportJob> ImportJobs { get; set; }
    }
}