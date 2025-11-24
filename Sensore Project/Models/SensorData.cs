using System;
using System.ComponentModel.DataAnnotations;

namespace Sensore_Project.Models
{
    public class SensorData
    {
        [Key]
        public int Id { get; set; }

        public double Pressure { get; set; }

        public DateTime Timestamp { get; set; }
    }
}