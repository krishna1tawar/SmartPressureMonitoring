using System;

namespace Sensore_Project.Services
{
    public class AnomalyResult
    {
        public bool IsAnomaly { get; set; }
        public double Score { get; set; }
    }

    public class AnomalyDetectionService
    {
        private const double MinSafePressure = 20.0;
        private const double MaxSafePressure = 120.0;

        public AnomalyResult CheckPressure(double pressure)
        {
            bool isLow = pressure < MinSafePressure;
            bool isHigh = pressure > MaxSafePressure;

            bool isAnomaly = isLow || isHigh;

            double score = 0.0;

            if (isAnomaly)
            {
                if (isLow)
                    score = (MinSafePressure - pressure) / MinSafePressure;
                else
                    score = (pressure - MaxSafePressure) / MaxSafePressure;

                if (score < 0) score = 0;
                if (score > 1) score = 1;
            }

            return new AnomalyResult
            {
                IsAnomaly = isAnomaly,
                Score = score
            };
        }
    }
}