using System.Linq;

namespace Sensore_Project.Models
{
    public class PressureMap
    {
        public int[][] Matrix { get; set; } = Array.Empty<int[]>();
        public string Scale { get; set; } = "1-255";
        public string Unit { get; set; } = "mmHg";

        // Validation: Ensure 32x32 matrix
        public bool IsValid()
        {
            if (Matrix == null || Matrix.Length != 32)
                return false;

            return Matrix.All(row => row != null && row.Length == 32);
        }
    }
}


