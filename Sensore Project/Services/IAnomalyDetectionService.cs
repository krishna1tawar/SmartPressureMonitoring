using Sensore_Project.Services;

namespace Sensore_Project.Services
{
    public interface IAnomalyDetectionService
    {
        AnomalyResult CheckPressure(double pressure);
    }
}
