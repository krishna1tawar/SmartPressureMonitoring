namespace Sensore_Project.Services
{
    /// <summary>
    /// Interface for anomaly detection service that checks pressure values.
    /// </summary>
    public interface IAnomalyDetectionService
    {
        /// <summary>
        /// Checks if a pressure value is anomalous.
        /// </summary>
        /// <param name="pressure">The pressure value to check.</param>
        /// <returns>An AnomalyResult containing the detection result and score.</returns>
        AnomalyResult CheckPressure(double pressure);
    }
}
