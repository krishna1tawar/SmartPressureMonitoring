using System;
using System.Threading.Tasks;
using Sensore_Project.Models;

namespace Sensore_Project.Services
{
    public class RiskPredictionService : IRiskPredictionService
    {
        public Task<RiskPrediction> PredictRiskAsync(double pressure)
        {
            string level;

            // Simple threshold logic that matches your unit tests
            if (pressure < 100)
                level = "Low";
            else if (pressure < 150)
                level = "Medium";
            else
                level = "High";

            var result = new RiskPrediction
            {
                Pressure = pressure,
                RiskScore = 0,        // no ML model, always 0
                RiskLevel = level,
                Timestamp = DateTime.UtcNow
            };

            return Task.FromResult(result);
        }
    }
}