namespace Sensore_Project.Models
{
    public class RiskPrediction
    {
        public int Id { get; set; }

        public double Pressure { get; set; }

        public double RiskScore { get; set; }   // ← REQUIRED (fixes error)

        public string RiskLevel { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}