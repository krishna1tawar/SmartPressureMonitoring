namespace Sensore_Project.Models.DTOs
{
    /// <summary>
    /// Request parameters for filtering alerts.
    /// </summary>
    public class AlertFilterRequest
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; } // "all", "active", "resolved"
    }
}
