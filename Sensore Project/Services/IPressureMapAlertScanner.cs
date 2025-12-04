using System.Threading;
using System.Threading.Tasks;
using Sensore_Project.Models;

namespace Sensore_Project.Services
{
    public interface IPressureMapAlertScanner
    {
        Task<ScanResult> ScanAsync(CancellationToken cancellationToken = default);
    }
}
