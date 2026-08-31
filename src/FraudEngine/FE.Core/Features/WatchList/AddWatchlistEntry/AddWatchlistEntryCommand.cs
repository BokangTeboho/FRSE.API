using FastEndpoints;
using FE.Core.Enums;

namespace FE.Core.Features.WatchList.AddWatchlistEntry
{
    public record AddWatchlistEntryCommand : ICommand<AddWatchlistEntryResult>
    {
        public required EntityType EntityType { get; init; }
        public required string EntityIdentifier { get; init; }
        public required Severity RiskLevel { get; init; }
        public required string Reason { get; init; }
    }

    public record AddWatchlistEntryResult(
        Guid Id,
        EntityType EntityType,
        string EntityIdentifier,
        Severity RiskLevel,
        string Reason,
        int AlertCount,
        bool IsManualEntry,
        string ModifiedByIdentifier,
        bool IsActive,
        DateTimeOffset CreatedAt);
}
