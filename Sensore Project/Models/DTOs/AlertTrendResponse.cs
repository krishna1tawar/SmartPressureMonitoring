namespace Sensore_Project.Models.DTOs
{
    /// <summary>
    /// Single data point for alert trend chart.
    /// </summary>
    public class AlertTrendPoint
    {
        public DateTime Timestamp { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Response containing alert trend data for charting.
    /// </summary>
    public class AlertTrendResponse
    {
        public List<AlertTrendPoint> DataPoints { get; set; } = new();
        public string Bucket { get; set; } = "hour"; // "hour" or "day"
    }
}
