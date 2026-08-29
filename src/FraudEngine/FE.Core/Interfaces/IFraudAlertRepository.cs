using FE.Core.Entities;

namespace FE.Core.Interfaces
{
    public interface IFraudAlertRepository
    {
        Task Add(FraudAlert fraudAlert, CancellationToken ct);
    }
}
