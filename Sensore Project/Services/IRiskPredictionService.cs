using System.Threading.Tasks;
using Sensore_Project.Models;

namespace Sensore_Project.Services
{
    public interface IRiskPredictionService
    {
        /// <summary>
        /// Existing single-value risk prediction (preserved for backward compatibility).
        /// </summary>
        Task<RiskPrediction> PredictRiskAsync(double pressure);

        /// <summary>
        /// Map-based risk prediction using the full 32x32 pressure map and pre-computed metrics.
        /// </summary>
        Task<RiskPrediction> PredictRiskFromMapAsync(PressureMap pressureMap, PressureMetrics metrics);

        /// <summary>
        /// Derive higher-level risk metrics and patterns from clusters + map.
        /// </summary>
        MapRiskMetrics AnalyzeRiskPatterns(PressureMap pressureMap, List<HighPressureCluster> clusters);
    }
}