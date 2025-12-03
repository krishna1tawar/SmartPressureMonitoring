using System.Collections.Generic;

namespace Sensore_Project.Models
{
    public class ClusterInfo
    {
        public List<HighPressureCluster> Clusters { get; set; } = new();
        public int TotalClusters { get; set; }
        public int TotalHighPressurePixels { get; set; }
    }
}


