using System.Collections.Generic;

namespace Sensore_Project.Models
{
    /// <summary>
    /// Contains information about high-pressure clusters detected in a pressure map.
    /// </summary>
    public class ClusterInfo
    {
        public List<HighPressureCluster> Clusters { get; set; } = new();
        public int TotalClusters { get; set; }
        public int TotalHighPressurePixels { get; set; }
    }
}
