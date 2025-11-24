using System.Collections.Generic;

namespace Sensore_Project.Models
{
    public class DashboardViewModel
    {
        public double LatestPressure { get; set; }
        public string LatestTimestamp { get; set; } = string.Empty;

        public double LatestRiskScore { get; set; }
        public string LatestRiskLevel { get; set; } = string.Empty;
    }
}