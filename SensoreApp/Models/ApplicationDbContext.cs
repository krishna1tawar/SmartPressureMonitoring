using Microsoft.EntityFrameworkCore;

namespace SensoreApp.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Common placeholder tables — each team member adds their own later
        // Example:
        // public DbSet<SensorData> SensorData { get; set; }
        // public DbSet<User> Users { get; set; }
    }
}