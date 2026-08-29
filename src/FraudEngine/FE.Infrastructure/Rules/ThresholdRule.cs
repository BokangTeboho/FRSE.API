using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;
using FE.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace FE.Infrastructure.Rules
{
    public record ThresholdRuleOptions
    {
        public Dictionary<string, decimal> Limits { get; init; } = [];
    }

    public class ThresholdRule(IOptions<ThresholdRuleOptions> options) : IFraudRule
    {
        public string Name => "Threshold";

        public FraudRuleResult Evaluate(Transaction transaction, ScanSnapshot snapshot)
        {
            var optionsValue = options.Value;

            optionsValue.Limits.TryGetValue(transaction.Currency, out var limit);

            if (transaction.Amount <= limit)
                return FraudRuleResult.Clean();

            var ratio = transaction.Amount / limit;

            var severity = ratio switch
            {
                >= 3 => Severity.Critical,
                >= 2 => Severity.High,
                _ => Severity.Medium
            };

            return FraudRuleResult.Triggered(
                ruleName: Name,
                severity: severity,
                description: $"Amount {transaction.Amount:C} exceeds threshold of {limit:C} ({ratio:F1}x)"
            );
        }
    }
}
