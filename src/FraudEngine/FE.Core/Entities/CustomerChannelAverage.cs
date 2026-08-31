using FE.Core.Enums;

namespace FE.Core.Entities
{
    public class CustomerChannelAverage
    {
        public Guid Id { get; init; }
        public Guid CustomerId { get; init; }
        public PaymentChannel PaymentChannel { get; init; }
        public decimal AverageAmount { get; set; }
        public int TransactionCount { get; set; }
    }
}
