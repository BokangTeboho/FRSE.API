using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;
using FE.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace FE.Infrastructure.Rules
{
    public class VelocityRuleOptions
    {
        public TimeSpan Window { get; init; }
        public int MaxTransactions { get; init; }
    }

    public class VelocityRule(IOptions<VelocityRuleOptions> options) : IFraudRule
    {
        public string Name => "Velocity";

        public FraudRuleResult Evaluate(Transaction transaction, ScanSnapshot snapshot)
        {
            var optionsValue = options.Value;
            var windowStart = transaction.CreatedAt - optionsValue.Window;

            var count = snapshot.RecentTransactions
                .Count(t => t.CreatedAt >= windowStart);

            if (count < optionsValue.MaxTransactions)
                return FraudRuleResult.Clean();

            var ratio = (double)count / optionsValue.MaxTransactions;

            var severity = ratio switch
            {
                >= 3 => Severity.Critical,
                >= 2 => Severity.High,
                _ => Severity.Medium
            };

            return FraudRuleResult.Triggered(
                ruleName: Name,
                severity: severity,
                description: $"{count} transactions in {optionsValue.Window.TotalMinutes} minutes (limit: {optionsValue.MaxTransactions})"
            );
        }
    }
}