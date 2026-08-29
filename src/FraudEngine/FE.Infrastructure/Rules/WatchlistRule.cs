using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;
using FE.Core.Interfaces;

namespace FE.Infrastructure.Rules
{
    public class WatchlistRule : IFraudRule
    {
        public string Name => "Watchlist";

        public IReadOnlySet<PaymentChannel> ApplicableChannels =>
            new HashSet<PaymentChannel>
            {
                PaymentChannel.CardPresent,
                PaymentChannel.Online,
                PaymentChannel.Transfer
            };

        public FraudRuleResult Evaluate(Transaction transaction, ScanSnapshot snapshot)
        {
            var entry = snapshot.MerchantWatchlistEntry;

            if (entry is null || !entry.IsActive)
                return FraudRuleResult.Clean();

            return FraudRuleResult.Triggered(
                ruleName: Name,
                severity: entry.RiskLevel,
                description: $"Merchant '{transaction.MerchantName}' is on the watchlist: {entry.Reason}"
            );
        }
    }
}
