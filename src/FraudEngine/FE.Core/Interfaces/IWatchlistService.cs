using FE.Core.Entities;
using FE.Core.Enums;

namespace FE.Core.Interfaces
{
    public interface IWatchlistService
    {
        Task<WatchlistEntry?> CheckMerchant(string merchantId, CancellationToken cancellationToken);
        Task<WatchlistEntry?> CheckBeneficiary(string BeneficiaryAccountNumber, CancellationToken cancellationToken);
        Task Add(WatchlistEntry entry, CancellationToken ct);
        Task<WatchlistEntry?> Deactivate(Guid id, CancellationToken ct);
    }
}
