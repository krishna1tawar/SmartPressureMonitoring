using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Sensore_Project.Models
{
    public class Alert
    {
        public int Id { get; set; }

        public int UserId { get; set; } = 1;   

        public string Message { get; set; } = string.Empty;

        public double Pressure { get; set; }  

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; }

        public int? PressureMapId { get; set; }

        public string AlertType { get; set; } = "PressureAnomaly";

        [Column(TypeName = "nvarchar(max)")]
        public string? ClusterInfoJson { get; set; }

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
    }
}