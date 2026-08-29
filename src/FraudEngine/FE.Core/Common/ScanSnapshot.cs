using FE.Core.Entities;

namespace FE.Core.Common
{
    public record ScanSnapshot
    {
        public required Customer Customer { get; init; }
        public IReadOnlyList<Transaction> RecentTransactions { get; init; } = [];
        public Transaction? LastTransaction => RecentTransactions.LastOrDefault();
        public WatchlistEntry? MerchantWatchlistEntry { get; init; }
        public WatchlistEntry? BeneficiaryWatchlistEntry { get; init; }
    }
}
