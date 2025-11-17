using Microsoft.EntityFrameworkCore;
using Sensore_Project.Models;

namespace Sensore_Project.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<SensorData> SensorData { get; set; }
        public DbSet<RiskPrediction> RiskPredictions { get; set; }

        // ✅ NEW — Add alerts table
        public DbSet<Alert> Alerts { get; set; }
    }
}