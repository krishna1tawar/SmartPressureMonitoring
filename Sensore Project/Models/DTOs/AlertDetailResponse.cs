namespace Sensore_Project.Models.DTOs
{
    /// <summary>
    /// Detailed alert response including pressure map data for modal view.
    /// </summary>
    public class AlertDetailResponse
    {
        public int Id { get; set; }
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public double Pressure { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsResolved { get; set; }
        public int? PressureMapId { get; set; }

        // Pressure map data (if available)
        public int[][]? PressureMapMatrix { get; set; }
        public string? MapScale { get; set; }
        public string? MapUnit { get; set; }

        // Cluster info (if available)
        public ClusterInfo? ClusterInfo { get; set; }

        // Metrics (if available)
        public PressureMetrics? Metrics { get; set; }
    }
}
