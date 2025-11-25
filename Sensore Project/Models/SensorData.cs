using System;
using System.ComponentModel.DataAnnotations;

namespace Sensore_Project.Models
{
    public class SensorData
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public double Pressure { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}