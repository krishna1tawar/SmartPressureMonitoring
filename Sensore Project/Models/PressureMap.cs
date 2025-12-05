using System.Linq;

namespace Sensore_Project.Models
{
    /// <summary>
    /// Represents a 32x32 pressure map matrix from a sensor.
    /// </summary>
    public class PressureMap
    {
        /// <summary>
        /// 32x32 matrix of pressure values (0-255).
        /// </summary>
        public int[][] Matrix { get; set; } = Array.Empty<int[]>();

        /// <summary>
        /// Scale description for the pressure values.
        /// </summary>
        public string Scale { get; set; } = "1-255";

        /// <summary>
        /// Unit of measurement for pressure values.
        /// </summary>
        public string Unit { get; set; } = "mmHg";

        /// <summary>
        /// Validates that the matrix is a proper 32x32 grid.
        /// </summary>
        public bool IsValid()
        {
            if (Matrix == null || Matrix.Length != 32)
                return false;

            return Matrix.All(row => row != null && row.Length == 32);
        }
    }
}
