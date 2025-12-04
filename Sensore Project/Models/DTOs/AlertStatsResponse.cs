namespace Sensore_Project.Models.DTOs
{
    /// <summary>
    /// Summary statistics for alerts in a time range.
    /// </summary>
    public class AlertStatsResponse
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Resolved { get; set; }
        public double MaxPressure { get; set; }
        public Dictionary<string, int> ByType { get; set; } = new();
    }
}
