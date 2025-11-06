using Microsoft.EntityFrameworkCore;
using SensoreApp.Models;

namespace Sensore_Project
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SensorData> SensorData { get; set; }
    }
}