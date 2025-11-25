using System.Threading.Tasks;
using Sensore_Project.Models;

namespace Sensore_Project.Services
{
    public interface IRiskPredictionService
    {
        Task<RiskPrediction> PredictRiskAsync(double pressure);
    }
}