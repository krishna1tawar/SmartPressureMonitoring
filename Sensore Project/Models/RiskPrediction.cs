using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Sensore_Project.Models
{
    public class RiskPrediction
    {
        public int Id { get; set; }

        public double Pressure { get; set; }  // Single value (backward compatibility)

        // Required for calculations & unit tests
        public double RiskScore { get; set; }

        public string RiskLevel { get; set; } = "Low";

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // NEW: Link to pressure map
        public int? PressureMapId { get; set; }

        // NEW: Analysis type
        public string AnalysisType { get; set; } = "SingleValue";

        // NEW: Map metrics as JSON
        [Column(TypeName = "nvarchar(max)")]
        public string? MapMetricsJson { get; set; }

        // Helper method for JSON deserialization
        [NotMapped]
        public MapRiskMetrics? MapMetrics
        {
            get => string.IsNullOrEmpty(MapMetricsJson) 
                ? null 
                : JsonSerializer.Deserialize<MapRiskMetrics>(MapMetricsJson);
            set => MapMetricsJson = value == null 
                ? null 
                : JsonSerializer.Serialize(value);
        }
    }
}