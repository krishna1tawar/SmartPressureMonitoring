using Microsoft.EntityFrameworkCore;
using Sensore_Project.Models;

namespace Sensore_Project
{
    /// <summary>
    /// Entity Framework database context for the Smart Pressure Monitoring application.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>Sensor data readings with optional pressure maps.</summary>
        public DbSet<SensorData> SensorData { get; set; }

        /// <summary>Alerts generated from anomaly detection or pressure map analysis.</summary>
        public DbSet<Alert> Alerts { get; set; }

        /// <summary>Risk prediction results.</summary>
        public DbSet<RiskPrediction> RiskPredictions { get; set; }

        /// <summary>CSV import job tracking.</summary>
        public DbSet<ImportJob> ImportJobs { get; set; }

        /// <summary>Comments on alerts with optional feedback.</summary>
        public DbSet<Comment> Comments { get; set; }
    }
}