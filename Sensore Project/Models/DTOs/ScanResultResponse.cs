using System.Collections.Generic;

namespace Sensore_Project.Models.DTOs
{
    public class ScanResultResponse
    {
        public int TotalMapsScanned { get; set; }
        public int AlertsCreated { get; set; }
        public int MapsSkipped { get; set; }
        public int Errors { get; set; }
        public List<string> ErrorMessages { get; set; } = new();
        public bool IsComplete { get; set; }
        public bool Cancelled { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
