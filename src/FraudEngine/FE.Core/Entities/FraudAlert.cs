using FE.Core.Enums;

namespace FE.Core.Entities
{
    public class FraudAlert
    {
        public Guid Id { get; set; }
        public Guid TransactionId { get; set; }
        public required string RuleName { get; set; }
        public Severity Severity { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
