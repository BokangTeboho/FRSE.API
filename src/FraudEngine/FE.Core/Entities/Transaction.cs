using FE.Core.Enums;

namespace FE.Core.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public decimal Amount { get; set; }
        public required string Currency { get; set; }
        public required string Country { get; set; }
        public string? MerchantName { get; set; }
        public string? Category { get; set; }
        public string? BeneficiaryId { get; set; }
        public PaymentTiming PaymentTiming { get; set; }
        public bool IsFlagged { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
