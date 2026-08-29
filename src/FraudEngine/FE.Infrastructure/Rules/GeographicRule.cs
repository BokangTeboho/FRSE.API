using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;
using FE.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace FE.Infrastructure.Rules
{
    public class GeographicRuleOptions
    {
        public TimeSpan MinTimeBetweenCountries { get; init; }
    }

    public class GeographicRule(IOptions<GeographicRuleOptions> options) : IFraudRule
    {
        public string Name => "Geographic";

        public IReadOnlySet<PaymentChannel> ApplicableChannels =>
            new HashSet<PaymentChannel> { PaymentChannel.CardPresent };

        public FraudRuleResult Evaluate(Transaction transaction, ScanSnapshot snapshot)
        {
            var optionsValue = options.Value;

            var last = snapshot.LastTransaction;

            if (last is null)
                return FraudRuleResult.Clean();

            if (last.Country == transaction.Country)
                return FraudRuleResult.Clean();

            var timeBetween = transaction.CreatedAt - last.CreatedAt;

            if (timeBetween >= optionsValue.MinTimeBetweenCountries)
                return FraudRuleResult.Clean();

            var severity = timeBetween.TotalMinutes switch
            {
                < 5 => Severity.Critical,
                < 30 => Severity.High,
                _ => Severity.Medium
            };

            return FraudRuleResult.Triggered(
                ruleName: Name,
                severity: severity,
                description: $"Transaction in {transaction.Country} just {timeBetween.TotalMinutes:F0} minutes after transaction in {last.Country}"
            );
        }
    }
}
