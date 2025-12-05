using System.Collections.Generic;
using Sensore_Project.Models;

namespace Sensore_Project.Services
{
    /// <summary>
    /// Analyses 32x32 pressure maps to detect high-pressure regions and compute metrics.
    /// </summary>
    public interface IPressureMapAnalysisService
    {
        /// <summary>
        /// Full analysis pipeline for a pressure map (cluster detection + metrics).
        /// </summary>
        PressureMetrics AnalyzePressureMap(PressureMap pressureMap);

        /// <summary>
        /// Detects contiguous high-pressure clusters in a matrix.
        /// </summary>
        List<HighPressureCluster> DetectHighPressureClusters(
            int[][] matrix,
            int threshold = 200,
            int minClusterSize = 10,
            bool useEightConnectivity = true);

        /// <summary>
        /// Returns true if the metrics warrant generating a user-visible alert.
        /// </summary>
        bool ShouldGenerateAlert(PressureMetrics metrics);
    }
}
