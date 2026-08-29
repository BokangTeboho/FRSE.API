using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;
using FE.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace FE.Infrastructure.Rules
{
    public class StructuringRuleOptions
    {
        public Dictionary<string, decimal> Thresholds { get; init; } = [];
        public decimal ProximityPercentage { get; init; }
    }

    public class StructuringRule(IOptions<StructuringRuleOptions> options) : IFraudRule
    {
        public string Name => "Structuring";

        public IReadOnlySet<PaymentChannel> ApplicableChannels => new HashSet<PaymentChannel>
        {
            PaymentChannel.CardPresent,
            PaymentChannel.Online,
            PaymentChannel.Transfer
        };

        public FraudRuleResult Evaluate(Transaction transaction, ScanSnapshot snapshot)
        {
            var optionsValue = options.Value;

            if (!optionsValue.Thresholds.TryGetValue(transaction.Currency, out var threshold))
                return FraudRuleResult.Clean();

            var floor = threshold * (1 - optionsValue.ProximityPercentage);
            var isNearThreshold = transaction.Amount >= floor && transaction.Amount < threshold;

            if (!isNearThreshold)
                return FraudRuleResult.Clean();

            var recentNearThresholdCount = snapshot.RecentTransactions
                .Count(t => t.Currency == transaction.Currency
                         && t.Amount >= floor
                         && t.Amount < threshold);

            if (recentNearThresholdCount == 0)
            {
                return FraudRuleResult.Triggered(
                    ruleName: Name,
                    severity: Severity.Low,
                    description: $"Amount {transaction.Amount:C} is just below reporting threshold of {threshold:C}"
                );
            }

            var severity = recentNearThresholdCount switch
            {
                >= 3 => Severity.Critical,
                >= 2 => Severity.High,
                _ => Severity.Medium
            };

            return FraudRuleResult.Triggered(
                ruleName: Name,
                severity: severity,
                description: $"Amount {transaction.Amount:C} is just below reporting threshold of {threshold:C}, with {recentNearThresholdCount} similar transactions recently"
            );
        }
    }
}
