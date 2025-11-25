using System;

namespace Sensore_Project.Models
{
    public class RiskPrediction
    {
        public int Id { get; set; }

        public double Pressure { get; set; }

        // Required for calculations & unit tests
        public double RiskScore { get; set; }

        public string RiskLevel { get; set; } = "Low";

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}