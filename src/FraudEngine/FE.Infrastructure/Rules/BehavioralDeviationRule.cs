using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;
using FE.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace FE.Infrastructure.Rules
{
    public class BehavioralDeviationRuleOptions
    {
        public decimal DeviationMultiplier { get; init; }
    }

    public class BehavioralDeviationRule(IOptions<BehavioralDeviationRuleOptions> options) : IFraudRule
    {
        public string Name => "BehavioralDeviation";

        public IReadOnlySet<PaymentChannel> ApplicableChannels => new HashSet<PaymentChannel>
        {
            PaymentChannel.CardPresent,
            PaymentChannel.Online,
            PaymentChannel.Transfer
        };

        public FraudRuleResult Evaluate(Transaction transaction, ScanSnapshot snapshot)
        {
            var optionsValue = options.Value;

            if (snapshot.ChannelAverage.AverageAmount <= 0)
                return FraudRuleResult.Clean();

            var ratio = transaction.Amount / snapshot.ChannelAverage.AverageAmount;

            if (ratio < optionsValue.DeviationMultiplier)
                return FraudRuleResult.Clean();

            var severity = ratio switch
            {
                >= 10 => Severity.Critical,
                >= 5 => Severity.High,
                _ => Severity.Medium
            };

            return FraudRuleResult.Triggered(
                ruleName: Name,
                severity: severity,
                description: $"Amount {transaction.Amount:C} is {ratio:F1}x the customer's average of {snapshot.ChannelAverage.AverageAmount:C}"
            );
        }
    }
}
