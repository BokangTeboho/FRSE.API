using FE.Core.Enums;

namespace FE.Core.Interfaces
{
    public interface IWatchlistService
    {
        Task<bool> IsMerchantBlacklisted(string merchantId);
        Task<bool> IsBeneficiaryBlacklisted(string beneficiaryId);
        Task<bool> AddEntity(string Id, EntityType type);
    }
}
