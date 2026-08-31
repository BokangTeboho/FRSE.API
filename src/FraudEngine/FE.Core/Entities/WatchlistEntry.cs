using FE.Core.Enums;

namespace FE.Core.Entities
{
    public class WatchlistEntry
    {
        public Guid Id { get; set; }
        public EntityType EntityType { get; set; }
        public required string EntityIdentifier { get; set; }
        public Severity RiskLevel { get; set; }
        public required string Reason { get; set; }
        public int AlertCount { get; set; }
        public bool IsManualEntry { get; set; }
        public required string ModifiedByIdentifier { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    }
}
