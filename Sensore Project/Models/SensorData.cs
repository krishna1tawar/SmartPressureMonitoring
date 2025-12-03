using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

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

        [Column(TypeName = "nvarchar(max)")]
        public string? PressureMapJson { get; set; }

        public bool RequiresClinicianReview { get; set; } = false;

        [Column(TypeName = "nvarchar(max)")]
        public string? MetricsJson { get; set; }

        [NotMapped]
        public PressureMap? PressureMap
        {
            get => string.IsNullOrEmpty(PressureMapJson) 
                ? null 
                : JsonSerializer.Deserialize<PressureMap>(PressureMapJson);
            set => PressureMapJson = value == null 
                ? null 
                : JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public PressureMetrics? Metrics
        {
            get => string.IsNullOrEmpty(MetricsJson) 
                ? null 
                : JsonSerializer.Deserialize<PressureMetrics>(MetricsJson);
            set => MetricsJson = value == null 
                ? null 
                : JsonSerializer.Serialize(value);
        }
    }
}