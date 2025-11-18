namespace SensoreApp.Services
{
    /// <summary>
    /// 🔍 Simple rule-based AI engine for pressure risk analysis.
    /// Later, you can replace this with a real ML model.
    /// </summary>
    public class RiskEngine
    {
        // Optional: configurable thresholds
        private const double LowPressureLimit = 60.0;
        private const double MediumPressureLimit = 80.0;
        private const double HighPressureLimit = 90.0;

        public string EvaluateRisk(double pressure, double? temperature = null)
        {
            if (pressure <= 0)
                return "Invalid";

            // Combine pressure & temperature for smarter results
            if (pressure >= HighPressureLimit || (temperature.HasValue && temperature > 38))
                return "High";

            if (pressure >= MediumPressureLimit)
                return "Medium";

            if (pressure >= LowPressureLimit)
                return "Low";

            return "Normal";
        }

        // Optional — numeric score for dashboards
        public double RiskScore(double pressure, double? temperature = null)
        {
            double baseScore = Math.Min(pressure / 100.0, 1.0);
            if (temperature.HasValue)
                baseScore += (temperature.Value - 25) * 0.01; // adds weight for heat
            return Math.Round(Math.Clamp(baseScore, 0, 1), 2);
        }
    }
}