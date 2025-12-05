using System.Collections.Generic;

namespace Sensore_Project.Models
{
    /// <summary>
    /// Higher-level risk metrics derived from pressure map analysis.
    /// </summary>
    public class MapRiskMetrics
    {
        public PatternAnalysis PatternAnalysis { get; set; } = new();
        public ClusterMetrics ClusterMetrics { get; set; } = new();
    }

    /// <summary>
    /// Analysis of pressure distribution patterns.
    /// </summary>
    public class PatternAnalysis
    {
        public bool HasConcentratedHighPressure { get; set; }
        public bool HasDistributedHighPressure { get; set; }
        public double PressureGradient { get; set; }
        public List<string> RiskPatterns { get; set; } = new();
    }

    /// <summary>
    /// Metrics about high-pressure clusters.
    /// </summary>
    public class ClusterMetrics
    {
        public int LargestClusterSize { get; set; }
        public int ClusterCount { get; set; }
        public double AvgClusterPressure { get; set; }
    }
}
