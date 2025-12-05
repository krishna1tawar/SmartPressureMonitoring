using System;
using Sensore_Project.Models;

namespace Sensore_Project.Models.DTOs
{
    /// <summary>
    /// Response DTO for risk prediction results.
    /// </summary>
    public class RiskPredictionResponse
    {
        public int Id { get; set; }
        public double RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string AnalysisType { get; set; } = string.Empty;
        public int? PressureMapId { get; set; }
        public DateTime Timestamp { get; set; }
        public MapRiskMetrics? MapMetrics { get; set; }
    }
}
