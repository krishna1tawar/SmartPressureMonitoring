using System;

namespace Sensore_Project.Services
{
    /// <summary>
    /// Result of an anomaly detection check.
    /// </summary>
    public class AnomalyResult
    {
        /// <summary>True if the value is outside safe thresholds.</summary>
        public bool IsAnomaly { get; set; }

        /// <summary>Normalised anomaly score (0-1).</summary>
        public double Score { get; set; }
    }

    /// <summary>
    /// Service for detecting pressure anomalies based on configurable thresholds.
    /// </summary>
    public class AnomalyDetectionService : IAnomalyDetectionService
    {
        private readonly double _minSafe;
        private readonly double _maxSafe;

        /// <summary>
        /// Creates an anomaly detection service with specified safe pressure thresholds.
        /// </summary>
        /// <param name="minSafe">Minimum safe pressure value.</param>
        /// <param name="maxSafe">Maximum safe pressure value.</param>
        public AnomalyDetectionService(double minSafe = 20.0, double maxSafe = 120.0)
        {
            _minSafe = minSafe;
            _maxSafe = maxSafe;
        }

        /// <summary>
        /// Checks if a pressure value is anomalous (outside safe thresholds).
        /// </summary>
        public AnomalyResult CheckPressure(double pressure)
        {
            bool isLow = pressure < _minSafe;
            bool isHigh = pressure > _maxSafe;

            bool isAnomaly = isLow || isHigh;
            double score = 0.0;

            if (isAnomaly)
            {
                double diff = isLow ? (_minSafe - pressure) : (pressure - _maxSafe);
                double maxDiff = Math.Max(_minSafe, _maxSafe);

                score = Math.Clamp(diff / maxDiff, 0.0, 1.0);
            }

            return new AnomalyResult
            {
                IsAnomaly = isAnomaly,
                Score = score
            };
        }
    }
}