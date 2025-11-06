namespace SensoreApp.Models
{
    public class RiskPrediction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public double RiskScore { get; set; } // 0-100 scale
        public string RiskLevel { get; set; } // Low, Medium, High
        public DateTime PredictedAt { get; set; }
    }
}