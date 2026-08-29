using FE.Core.Enums;

namespace FE.Core.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public required string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public required string Currency { get; set; }
        public required string Country { get; set; }
        public PaymentChannel PaymentChannel { get; set; }
        public string? MerchantName { get; set; }
        public string? MerchantId { get; set; }
        public string? Category { get; set; }
        public string? BeneficiaryAccountNumber { get; set; }
        public PaymentTiming PaymentTiming { get; set; }
        public bool IsFlagged { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
