using System;

namespace Sensore_Project.Services
{
    public class AnomalyResult
    {
        public bool IsAnomaly { get; set; }
        public double Score { get; set; }   // 0–1 normalised
    }

    public class AnomalyDetectionService : IAnomalyDetectionService
    {
        private readonly double _minSafe;
        private readonly double _maxSafe;

        public AnomalyDetectionService(double minSafe = 20.0, double maxSafe = 120.0)
        {
            _minSafe = minSafe;
            _maxSafe = maxSafe;
        }

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