using System;
using Sensore_Project.Models;

namespace Sensore_Project.Models.DTOs
{
    public class AlertResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public int? PressureMapId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsResolved { get; set; }
        public ClusterInfo? ClusterInfo { get; set; }
    }
}

