using Sensore_Project.Models;

namespace Sensore_Project.Services
{
    /// <summary>
    /// Result returned by the anomaly check.
    /// </summary>
    public class AnomalyResult
    {
        public bool IsAnomaly { get; set; }
        public double Score { get; set; }   // 0 = normal, up to 1 = very abnormal
    }

    /// <summary>
    /// Simple pressure anomaly detection service.
    /// You can swap the logic with a full ML model later if you want.
    /// </summary>
    public class AnomalyDetectionService
    {
        // You can tweak these thresholds later
        private const double MinSafePressure = 20.0;
        private const double MaxSafePressure = 120.0;

        /// <summary>
        /// Check one pressure value and return anomaly info.
        /// </summary>
        public AnomalyResult CheckPressure(double pressure)
        {
            bool isLow = pressure < MinSafePressure;
            bool isHigh = pressure > MaxSafePressure;

            bool isAnomaly = isLow || isHigh;

            // Build a simple "how bad" score between 0 and 1
            double score = 0.0;

            if (isAnomaly)
            {
                double distance = isHigh
                    ? pressure - MaxSafePressure
                    : MinSafePressure - pressure;

                // Arbitrary scaling so very extreme values approach 1.0
                score = Math.Min(1.0, distance / 50.0);
            }

            return new AnomalyResult
            {
                IsAnomaly = isAnomaly,
                Score = score
            };
        }
    }
}