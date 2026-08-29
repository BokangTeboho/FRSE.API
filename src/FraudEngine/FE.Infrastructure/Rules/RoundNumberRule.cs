using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;
using FE.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace FE.Infrastructure.Rules
{
    public class RoundNumberRuleOptions
    {
        public decimal MinimumAmount { get; init; }
        public decimal RoundingIncrement { get; init; }
    }

    public class RoundNumberRule(IOptions<RoundNumberRuleOptions> options) : IFraudRule
    {
        public string Name => "RoundNumber";

        public IReadOnlySet<PaymentChannel> ApplicableChannels => new HashSet<PaymentChannel>
        {
            PaymentChannel.CardPresent,
            PaymentChannel.Online,
            PaymentChannel.Transfer
        };

        public FraudRuleResult Evaluate(Transaction transaction, ScanSnapshot snapshot)
        {
            var optionsValue = options.Value;

            if (transaction.Amount < optionsValue.MinimumAmount)
                return FraudRuleResult.Clean();

            if (transaction.Amount % optionsValue.RoundingIncrement != 0)
                return FraudRuleResult.Clean();

            var recentRoundCount = snapshot.RecentTransactions
                .Count(t => t.Amount >= optionsValue.MinimumAmount
                         && t.Amount % optionsValue.RoundingIncrement == 0);

            if (recentRoundCount == 0)
            {
                return FraudRuleResult.Triggered(
                    ruleName: Name,
                    severity: Severity.Low,
                    description: $"Exact round amount of {transaction.Amount:C}"
                );
            }

            var severity = recentRoundCount switch
            {
                >= 3 => Severity.Critical,
                >= 2 => Severity.High,
                _ => Severity.Medium
            };

            return FraudRuleResult.Triggered(
                ruleName: Name,
                severity: severity,
                description: $"Exact round amount of {transaction.Amount:C}, with {recentRoundCount} recent round-number transactions"
            );
        }
    }    
}
