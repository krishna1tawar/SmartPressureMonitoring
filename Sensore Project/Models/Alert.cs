using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Sensore_Project.Models
{
    /// <summary>
    /// Represents an alert generated from anomaly detection or pressure map analysis.
    /// </summary>
    public class Alert
    {
        public int Id { get; set; }

        public int UserId { get; set; } = 1;

        public string Message { get; set; } = string.Empty;

        public double Pressure { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; }

        /// <summary>Foreign key to the SensorData row containing the pressure map.</summary>
        public int? PressureMapId { get; set; }

        public string AlertType { get; set; } = "PressureAnomaly";

        /// <summary>JSON-serialized cluster information (stored in database).</summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? ClusterInfoJson { get; set; }

        /// <summary>Deserialized cluster information (not mapped to database).</summary>
        [NotMapped]
        public ClusterInfo? ClusterInfo
        {
            get => string.IsNullOrEmpty(ClusterInfoJson)
                ? null
                : JsonSerializer.Deserialize<ClusterInfo>(ClusterInfoJson);
            set => ClusterInfoJson = value == null
                ? null
                : JsonSerializer.Serialize(value);
        }

        /// <summary>Comments and feedback on this alert.</summary>
        public List<Comment> Comments { get; set; } = new();
    }
}