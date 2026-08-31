using FastEndpoints;
using FE.Core.Common;
using FE.Core.Enums;

namespace FE.Core.Features.Transaction.ScanTransaction
{
    public record ScanTransactionCommand : ICommand<ScanTransactionResult>
    {
        public required string ReferenceId { get; init; }
        public required string AccountNumber { get; init; }
        public required string CustomerName { get; init; }
        public required decimal Amount { get; init; }
        public required string Currency { get; init; }
        public required string Country { get; init; }
        public required PaymentChannel PaymentChannel { get; init; }
        public required PaymentTiming PaymentTiming { get; init; }
        public string? MerchantName { get; init; }
        public string? MerchantId { get; init; }
        public string? BeneficiaryAccountNumber { get; init; }
        public string? Category { get; init; }
    }

    public record ScanTransactionResult(string ReferenceId, IList<FraudRuleResult> TriggeredRules);
}
