namespace Sensore_Project.Models.DTOs
{
    public class PressureMapRequest
    {
        public int[][] Matrix { get; set; } = Array.Empty<int[]>();
        public string? Scale { get; set; } = "1-255";
        public string? Unit { get; set; } = "mmHg";
    }
}


