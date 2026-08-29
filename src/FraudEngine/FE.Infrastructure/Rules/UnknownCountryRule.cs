using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;
using FE.Core.Interfaces;

namespace FE.Infrastructure.Rules
{
    public class UnknownCountryRule : IFraudRule
    {
        public string Name => "UnknownCountry";

        public IReadOnlySet<PaymentChannel> ApplicableChannels => new HashSet<PaymentChannel>
        {
            PaymentChannel.CardPresent
        };

        public FraudRuleResult Evaluate(Transaction transaction, ScanSnapshot snapshot)
        {
            if (!snapshot.Customer.KnownCountries.Any())
                return FraudRuleResult.Clean();

            if (snapshot.Customer.KnownCountries.Contains(transaction.Country))
                return FraudRuleResult.Clean();

            return FraudRuleResult.Triggered(
                ruleName: Name,
                severity: Severity.Medium,
                description: $"Transaction from {transaction.Country}, not in customer's known countries"
            );
        }
    }
}
