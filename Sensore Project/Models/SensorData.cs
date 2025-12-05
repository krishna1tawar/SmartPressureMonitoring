using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Sensore_Project.Models
{
    /// <summary>
    /// Represents a sensor data reading, optionally including a 32x32 pressure map.
    /// </summary>
    public class SensorData
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Representative pressure value (average or single reading).</summary>
        [Required]
        public double Pressure { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>JSON-serialized 32x32 pressure map (stored in database).</summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? PressureMapJson { get; set; }

        /// <summary>Flag indicating this reading needs clinician review.</summary>
        public bool RequiresClinicianReview { get; set; } = false;

        /// <summary>JSON-serialized analysis metrics (stored in database).</summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? MetricsJson { get; set; }

        /// <summary>Deserialized pressure map (not mapped to database).</summary>
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

        /// <summary>Deserialized analysis metrics (not mapped to database).</summary>
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